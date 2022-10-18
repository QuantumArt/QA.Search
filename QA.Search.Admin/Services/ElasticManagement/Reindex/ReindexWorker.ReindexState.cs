using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    /// <summary>
    /// Класс, содержащий сведения об индексах эластик и 
    /// задачах реиндексирования, связанных с ними
    /// </summary>
    public partial class ReindexWorker : IReindexStateStore
    {
        private class ReindexState : IReindexState
        {
            

            private List<IIndexesContainer> Containers { get; set; }

            private ReaderWriterLockSlim InnerLock { get; }

            public ReindexState()
            {
                InnerLock = new ReaderWriterLockSlim();
                Containers = new List<IIndexesContainer>();
                Loaded = false;
            }

            #region IReindexState
            public bool Loaded { get; private set; }

            public bool CommonError { get; private set; }

            public IEnumerable<IIndexesContainer> GetAllContainers()
            {
                try
                {
                    InnerLock.EnterReadLock();
                    return new List<IIndexesContainer>(Containers);
                }
                finally
                {
                    InnerLock.ExitReadLock();
                }
            }

            #endregion

            private IIndexesContainer GetContainerByIndexFullName(string indexFullName)
            {
                if (string.IsNullOrWhiteSpace(indexFullName))
                {
                    return null;
                }
                try
                {
                    InnerLock.EnterReadLock();
                    return Containers.FirstOrDefault(c => c.SourceIndex?.FullName == indexFullName);
                }
                finally
                {
                    InnerLock.ExitReadLock();
                }
            }



            public void UpdateContainer(
                IIndexesContainer container,
                IElasticIndex sourceIndex,
                IElasticIndex destinationIndex,
                IEnumerable<IElasticIndex> wrongIndexes,
                IReindexTask reindexTask)
            {
                try
                {
                    InnerLock.EnterUpgradeableReadLock();
                    var oldContainer = Containers.FirstOrDefault(c => c.HasSourceIndex(container.SourceIndex.FullName));
                    if (oldContainer == null)
                    {
                        return;
                    }
                    var newContainer = new IndexesContainer(wrongIndexes, oldContainer.LastFinishedReindexTask)
                    {
                        SourceIndex = sourceIndex,
                        DestinationIndex = destinationIndex,
                        ReindexTask = reindexTask
                    };
                    try
                    {
                        InnerLock.EnterWriteLock();
                        Containers.Remove(oldContainer);
                        Containers.Add(newContainer);
                    }
                    finally
                    {
                        InnerLock.ExitWriteLock();
                    }
                }
                finally
                {
                    InnerLock.ExitUpgradeableReadLock();
                }
            }

            public void UpdateState(IEnumerable<IIndexesContainer> data)
            {
                try
                {
                    InnerLock.EnterWriteLock();
                    Containers.Clear();
                    if (data != null)
                    {
                        Containers.AddRange(data);
                        CommonError = false;
                    }
                    else
                    {
                        CommonError = true;
                    }
                    Loaded = true;
                }
                finally
                {
                    InnerLock.ExitWriteLock();

                }
            }

            private IIndexesContainer FindByIndexFullName(string indexFullName)
            {
                return Containers.FirstOrDefault(c =>
                        (c.SourceIndex != null && c.SourceIndex.FullName == indexFullName)
                        || (c.DestinationIndex != null && c.DestinationIndex.FullName == indexFullName)
                        || (c.WrongIndexes?.Any(i => i.FullName == indexFullName) ?? false));
            }
            private IIndexesContainer FindByIndexUIName(string indexUIName)
            {
                return Containers.FirstOrDefault(c =>
                        (c.SourceIndex != null && c.SourceIndex.UIName == indexUIName)
                        || (c.DestinationIndex != null && c.DestinationIndex.UIName == indexUIName)
                        || (c.WrongIndexes?.Any(i => i.UIName == indexUIName) ?? false));
            }

            internal void RemoveIndexFromContainer(string indexFullName)
            {
                try
                {
                    InnerLock.EnterWriteLock();
                    var targetContainer = FindByIndexFullName(indexFullName);
                    if (targetContainer == null)
                    {
                        return;
                    }
                    // Если удаляем исходный индекс
                    if (targetContainer.SourceIndex != null &&
                        targetContainer.SourceIndex.FullName == indexFullName)
                    {
                        if (targetContainer.DestinationIndex == null)
                        {
                            Containers.Remove(targetContainer);
                            return;
                        }
                        ((IndexesContainer)targetContainer).SourceIndex = targetContainer.DestinationIndex;
                        ((IndexesContainer)targetContainer).DestinationIndex = null;
                        return;
                    }
                    if (targetContainer.DestinationIndex != null &&
                        targetContainer.DestinationIndex.FullName == indexFullName)
                    {
                        ((IndexesContainer)targetContainer).DestinationIndex = null;
                        return;
                    }
                    var cn = targetContainer.WrongIndexes?.FirstOrDefault(wi => wi.FullName == indexFullName);
                    if (cn != null)
                    {
                        targetContainer.WrongIndexes.Remove(cn);
                        if (targetContainer.WrongIndexes.Count == 1)
                        {
                            ((IndexesContainer)targetContainer).SourceIndex = targetContainer.WrongIndexes.First();
                            targetContainer.WrongIndexes.Clear();
                        }
                    }
                }
                finally
                {
                    InnerLock.ExitWriteLock();
                }
            }

            internal void CreateNewContainerForIndex(IElasticIndex newIndex)
            {
                try
                {
                    InnerLock.EnterWriteLock();
                    var targetContainer = FindByIndexUIName(newIndex.UIName);
                    if (targetContainer == null)
                    {
                        var newContainer = new IndexesContainer(newIndex);
                        Containers.Add(newContainer);
                        return;
                    }

                    if (targetContainer.SourceIndex != null || targetContainer.DestinationIndex != null)
                    {
                        targetContainer.WrongIndexes.Add(newIndex);
                    }
                    if (targetContainer.SourceIndex != null)
                    {
                        targetContainer.WrongIndexes.Add(targetContainer.SourceIndex);
                        ((IndexesContainer)targetContainer).SourceIndex = null;
                    }
                    if (targetContainer.DestinationIndex != null)
                    {
                        targetContainer.WrongIndexes.Add(targetContainer.DestinationIndex);
                        ((IndexesContainer)targetContainer).DestinationIndex = null;
                    }
                }
                finally
                {
                    InnerLock.ExitWriteLock();
                }
                
            }
        }

        #region IReindexStateStore

        public IReindexState GetState() => CurrentState;

        #endregion

    }
}
