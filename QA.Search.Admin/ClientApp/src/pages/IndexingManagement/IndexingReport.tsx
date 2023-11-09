import React from "react";
import { Card, HTMLTable } from "@blueprintjs/core";

import { IndexingReportModel } from "../../backend.generated";

type Props = {
  report: IndexingReportModel | null;
};

function IndexingReport({ report }: Props) {
  // Данный компонент просто отображает сведения об отчете, поэтому данные передаются через props и контейнер не используется
  if (!report) {
    return null;
  }
  return (
    <Card elevation={2}>
      <h5 className="bp3-heading">Отчет</h5>
      <HTMLTable bordered condensed>
        <tbody>
          {!!report.indexName && (
            <tr>
              <td>Название индекса</td>
              <td>{report.indexName}</td>
            </tr>
          )}
          {!!report.batchSize && (
            <tr>
              <td>Размер партии</td>
              <td>{report.batchSize}</td>
            </tr>
          )}
          <tr>
            <td>Время загрузки продуктов</td>
            <td>{report.documentsLoadTime}</td>
          </tr>
          <tr>
            <td>Время обработки продуктов</td>
            <td>{report.documentsProcessTime}</td>
          </tr>
          <tr>
            <td>Время индексации продуктов</td>
            <td>{report.documentsIndexTime}</td>
          </tr>
          <tr>
            <td>Количество найденных продуктов</td>
            <td>{report.idsLoaded}</td>
          </tr>
          <tr>
            <td>Продуктов загружено</td>
            <td>{report.productsLoaded}</td>
          </tr>
          <tr>
            <td>Продуктов проиндексировано</td>
            <td>{report.productsIndexed}</td>
          </tr>
        </tbody>
      </HTMLTable>
    </Card>
  );
}

export default IndexingReport;
