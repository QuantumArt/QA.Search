export class SearchClient {
  private readonly _baseUrl: string;

  constructor(baseUrl: string = "") {
    this._baseUrl = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
  }

  /**
   * Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск
   * @throws {SearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public search<TModel>(
    signal: AbortSignal,
    request: SearchRequest<TModel>
  ): Promise<SearchResponse<TModel>>;
  public search<TModel>(request: SearchRequest<TModel>): Promise<SearchResponse<TModel>>;
  public search(signal: any, request?: any) {
    if (!request) {
      request = signal;
      signal = undefined;
    }
    return processSingleRequest(request, this._baseUrl + "/api/v1/search", signal);
  }

  /**
   * Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск
   * @throws {SearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public searchObsolete<TModel>(
    signal: AbortSignal,
    request: SearchRequest<TModel>
  ): Promise<SearchResponse<TModel>>;
  public searchObsolete<TModel>(request: SearchRequest<TModel>): Promise<SearchResponse<TModel>>;
  public searchObsolete(signal: any, request?: any) {
    if (!request) {
      request = signal;
      signal = undefined;
    }
    return processSingleRequest(request, this._baseUrl + "/api/v1/search/obsolete", signal);
  }

  /**
   * Поиск документов по префиксам слов в текстовых полях
   * @throws {SearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public suggest<TModel>(
    signal: AbortSignal,
    request: SuggestRequest<TModel>
  ): Promise<SuggestResponse<TModel>>;
  public suggest<TModel>(request: SuggestRequest<TModel>): Promise<SuggestResponse<TModel>>;
  public suggest(signal: any, request?: any) {
    if (!request) {
      request = signal;
      signal = undefined;
    }
    return processSingleRequest(request, this._baseUrl + "/api/v1/suggest", signal);
  }

  /**
   * Поиск подсказок по префиксам слов в текстовых полях
   * @throws {SearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public queryCompletions(
    signal: AbortSignal,
    request: QueryCompletionRequest
  ): Promise<SearchResponse<QueryCompletion>>;
  public queryCompletions(request: QueryCompletionRequest): Promise<SearchResponse<QueryCompletion>>;
  public queryCompletions(signal: any, request?: any) {
    if (!request) {
      request = signal;
      signal = undefined;
    }
    return processSingleRequest(request, this._baseUrl + "/api/v1/completion/query", signal);
  }

  /**
   * Регистрация введенного запроса пользователя
   * @throws {SearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public registerQueryCompletion<TModel>(
    signal: AbortSignal,
    request: QueryCompletionRegistrationRequest
  ): Promise<void>;
  public registerQueryCompletion<TModel>(request: QueryCompletionRegistrationRequest): Promise<void>;
  public registerQueryCompletion(signal: any, request?: any) {
    if (!request) {
      request = signal;
      signal = undefined;
    }
    return processSingleRequest(request, this._baseUrl + "/api/v1/completion/query_register", signal);
  }

  /**
   * Дополнение строки поискового ввода
   * @throws {SearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public completion<TModel>(
    signal: AbortSignal,
    request: CompletionRequest<TModel>
  ): Promise<CompletionResponse>;
  public completion<TModel>(request: CompletionRequest<TModel>): Promise<CompletionResponse>;
  public completion(signal: any, request?: any) {
    if (!request) {
      request = signal;
      signal = undefined;
    }
    return processSingleRequest(request, this._baseUrl + "/api/v1/completion", signal);
  }

  /**
   * Полнотекстовый поиск документов с синонимами и морфологией, фасетный поиск: мультиплексирование запросов
   * @throws {MultiSearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public multiSearch<TModels extends any[]>(
    signal: AbortSignal,
    requests: SearchRequests<TModels>
  ): Promise<SearchResponses<TModels>>;
  public multiSearch<TModels extends any[]>(
    requests: SearchRequests<TModels>
  ): Promise<SearchResponses<TModels>>;
  public multiSearch(signal: any, requests?: any) {
    if (!requests) {
      requests = signal;
      signal = undefined;
    }
    return processMultiRequest(requests, this._baseUrl + "/api/v1/multi_search", signal);
  }

  /**
   * Поиск документов по префиксам слов в текстовых полях: мультиплексирование запросов
   * @throws {MultiSearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public multiSuggest<TModels extends any[]>(
    signal: AbortSignal,
    requests: SuggestRequests<TModels>
  ): Promise<SuggestResponses<TModels>>;
  public multiSuggest<TModels extends any[]>(
    requests: SuggestRequests<TModels>
  ): Promise<SuggestResponses<TModels>>;
  public multiSuggest(signal: any, requests?: any) {
    if (!requests) {
      requests = signal;
      signal = undefined;
    }
    return processMultiRequest(requests, this._baseUrl + "/api/v1/multi_suggest", signal);
  }

  /**
   * Дополнение строки поискового ввода: мультиплексирование запросов
   * @throws {MultiSearchError} Ошибка при обращении к серверу
   * @throws {DOMException} [name="AbortError"] Запрос был отменен на клиенте
   */
  public multiCompletion<TModels extends any[]>(
    signal: AbortSignal,
    requests: CompletionRequests<TModels>
  ): Promise<CompletionResponses<TModels>>;
  public multiCompletion<TModels extends any[]>(
    requests: CompletionRequests<TModels>
  ): Promise<CompletionResponses<TModels>>;
  public multiCompletion(signal: any, requests?: any) {
    if (!requests) {
      requests = signal;
      signal = undefined;
    }
    return processMultiRequest(requests, this._baseUrl + "/api/v1/multi_completion", signal);
  }
}

export default SearchClient;

async function processSingleRequest(
  requests: BaseRequest<any>[],
  url: string,
  signal?: AbortSignal
): Promise<any> {
  const response = await fetch(url, {
    method: "POST",
    body: JSON.stringify(requests),
    credentials: "include",
    signal,
    headers: { "Content-Type": "application/json" }
  });

  if (response.ok) {
    const text = await response.text();
    return text ? JSON.parse(text, parseJsonDates) : {};
  }

  const responseType = response.headers.get("Content-Type");

  if (responseType && responseType.startsWith("application/json")) {
    throw new SearchError(await response.json());
  }

  throw new SearchError({ status: response.status });
}

async function processMultiRequest(
  requests: BaseRequest<any>[],
  url: string,
  signal?: AbortSignal
): Promise<any> {
  const response = await fetch(url, {
    method: "POST",
    body: JSON.stringify(requests),
    credentials: "include",
    signal,
    headers: { "Content-Type": "application/json" }
  });

  if (response.ok) {
    const text = await response.text();
    const results: { status: number }[] = JSON.parse(text, parseJsonDates);

    if (results.every(result => result.status === 200)) {
      return results;
    }

    const pairs: [ProblemDetails, BaseRequest<any>][] = [];

    results
      .filter(result => result.status !== 200)
      .forEach((result, i) => {
        pairs.push([result, requests[i]]);
      });

    throw new MultiSearchError({ status: response.status }, pairs);
  }

  const responseType = response.headers.get("Content-Type");

  if (responseType && responseType.startsWith("application/json")) {
    throw new MultiSearchError(await response.json());
  }

  throw new MultiSearchError({ status: response.status });
}

const dataRateRegexForUnspecifiedKind = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(.\d+)*$/;
const iso8601DateRegex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:((\.\d+)?Z)|((\.\d+)?\+\d{2}:\d{2}))?$/;

/**
 * Примеры для проверки
 * 2018-02-14T23:32:56.5987719+03:00
 * 2018-02-10T09:42:14.4575689Z
 * 2018-03-12T10:46:32.123
 */
function parseJsonDates(_key: string, value: any) {
  if (typeof value === "string" && dataRateRegexForUnspecifiedKind.test(value)) {
    return new Date(value + "Z");
  }
  if (typeof value === "string" && iso8601DateRegex.test(value)) {
    return new Date(value);
  }
  return value;
}

type SearchRequests<TArgs extends any[]> = { [K in keyof TArgs]: SearchRequest<TArgs[K]> };
type SearchResponses<TArgs extends any[]> = { [K in keyof TArgs]: SearchResponse<TArgs[K]> };
type SuggestRequests<TArgs extends any[]> = { [K in keyof TArgs]: SuggestRequest<TArgs[K]> };
type SuggestResponses<TArgs extends any[]> = { [K in keyof TArgs]: SuggestResponse<TArgs[K]> };
type CompletionRequests<TArgs extends any[]> = { [K in keyof TArgs]: CompletionRequest<TArgs[K]> };
type CompletionResponses<TArgs extends any[]> = { [K in keyof TArgs]: CompletionResponse };

interface ProblemDetails {
  status: number;
  title?: string;
  type?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export class SearchError extends Error implements ProblemDetails {
  status: number;
  title?: string;
  type?: string;
  detail?: string;
  errors?: Record<string, string[]>;
  constructor(details: ProblemDetails) {
    super(details.title || details.detail);
    this.status = details.status;
    Object.assign(this, details);
  }
}

export class MultiSearchError extends SearchError {
  items?: [ProblemDetails, BaseRequest<any>][];
  constructor(details: ProblemDetails, items?: [ProblemDetails, BaseRequest<any>][]) {
    super(details);
    this.items = items;
  }
}

type Primitive = null | boolean | number | string | Date;
type UnwrapArr<T> = T extends Primitive ? T : T extends any[] ? T[0] : T;
type NonEmptyArr<T> = [T, ...T[]];

/**
 * Условие фильтрации по отдельному полю.
 * Скалярное значение преобразуется в { "$eq" }, массив значений — в { "$in" }.
 * Условия внутри одного объекта объединяются через AND.
 * Условия внутри массива объектов объединяются через OR.
 * @example
 * { "$in": ["moskva", "spb"] }
 * { "$gt": "2015-01-01" }
 */
type ConditionExpr<P> =
  | P
  | {
      /** Равно */
      $eq?: P;
      /** Не равно */
      $ne?: P;
      /** Содержит одно из @alias $any */
      $in?: P[];
      /** Содержит одно из @alias $in */
      $any?: P[];
      /** Содержит все */
      $all?: P[];
      /** Не содержит ни одного из */
      $none?: P[];
      /** Меньше */
      $lt?: P;
      /** Меньше или равно */
      $lte?: P;
      /** Больше */
      $gt?: P;
      /** Больше или равно */
      $gte?: P;
    };

/**
 * Условия фильтрации по набору полей объекта.
 * Условия внутри одного объекта объединяются через AND
 * @example
 * {
 *   "Region": { "Alias": "moskva" },
 *   "PublishDate": { "$gt": "2015-01-01" }
 * }
 */
type FilterExpr<T> = T extends Primitive
  ? ConditionExpr<T> | ConditionExpr<T>[]
  :
      | {
          [K in keyof T]?: FilterExpr<UnwrapArr<T[K]>>;
        }
      | {
          [K in string]: FilterExpr<any>;
        };

/**
 * Набор условий для фильтрации документов.
 * Условия внутри одного объекта объединяются через AND
 * @example
 * {
 *   "$some": [{
 *     "Region.Alias": ["moskva", "spb"],
 *     "PublishDate": { "$gt": "2014-12-31" }
 *   }, {
 *     "Region.Alias": "vladivostok",
 *     "PublishDate": { "$gt": "2015-01-01" }
 *   }],
 *   "$exists": "MarketingProduct.Parameters",
 *   "$where": {
 *     "BaseParameter.Alias": "price",
 *     "NumValue": { "$lt": 1000 }
 *   }
 * }
 */
type WhereExpr<T> = FilterExpr<T> & {
  /** Выполняются все указанные условия */
  $every?: WhereExpr<T>[];
  /** Выполняется хотя бы одно из указанных условий */
  $some?: WhereExpr<T>[];
  /** Не выполняется указанное условие */
  $not?: WhereExpr<T>;
  /**
   * По указанному пути содержится хотя бы один 'nested' документ
   * https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-nested-query.html
   */
  $exists?: keyof T | string;
  /**
   * Условия фильтрации для 'nested' документа. Bсе имена полей в условии должны
   * иметь префикс, указанный в @see $exists
   */
  $where?: WhereExpr<T>;
};

/**
 * Веса полей для полнотекстового поиска
 * @example
 * {
 *   "Title": 5,
 *   "Body": 1,
 *   "Groups": { "Title": 2 }
 * }
 */
type WeightsExpr<T> = T extends Primitive
  ? number
  :
      | {
          [K in keyof T]?: WeightsExpr<UnwrapArr<T[K]>>;
        }
      | {
          [K in string]: WeightsExpr<any>;
        };

/** Настройка подсвеченных фрагментов по одному полю */
type SnippetExpr =
  | number
  | {
      /**
       * Кол-во подсвеченных фрагментов по одному полю @default 5
       * Если указан `0`, то подсвечивается поле целиком.
       */
      $count?: number;
      /** Максимальная длина подвсеченного фрагмента @default 100 */
      $length?: number;
    };

/**
 * Настройка подсвеченных фрагментов по выбранным полям.
 * Если поля не указаны, используется псевдо-поле "_all",
 * включающее все полнотекстовые поля документа
 * @example
 * {
 *   "Title": { "$count": 1, "$length": 120 },
 *   "Body": { "$count": 2, "$length": 300 }
 * }
 */
type SnippetsExpr<T> = T extends Primitive
  ? SnippetExpr
  :
      | {
          [K in keyof T]?: SnippetsExpr<UnwrapArr<T[K]>>;
        }
      | {
          [K in string]: SnippetsExpr<any>;
        };

/**
 * Порядок сортировки результатов: имя поля, или путь к вложенному полю через точку
 * или объект вида { "filed_name": "asc" | "desc" }
 * @example
 * "Regions.Title"
 * { "Regions.Alias": "desc" }
 */
type OrderByExpr<T> =
  | keyof T
  | string
  | {
      [K in keyof T]: T[K] extends Primitive ? "asc" | "desc" : never;
    }
  | {
      [K in string]: "asc" | "desc";
    };

/**
 * Условия исправления поискового запроса и результатов выдачи по исправленному запросу
 * @example
 * {
 *   "$query": { "$ifFoundLte": 10 },
 *   "$results": { "$ifFoundLte": 5 }
 * }
 */
type CorrectExpr = {
  /** Исправить строку поискового запроса, если найдено не больше */
  $query?: {
    /** Исправить, если найдено не больше */
    $ifFoundLte: number;
  };
  /** Исправить результаты выдачи, если найдено не больше */
  $results?: {
    /** Исправить, если найдено не больше */
    $ifFoundLte: number;
  };
};

type SamplesFacetExpr = {
  /**
   * Нахождение указанного числа наиболее популярных значений поля в выборке
   * с подсчетом количества документов, соответствущих этому значению.
   * @example
   * "$samples"
   * { "$samples": 10 }
   */
  $samples: number;
};

type RangesFacetExpr = {
  /**
   * Подсчет количества документов в каждой группе, определяемой
   * верхней и нижней границами для значения указанного поля
   * @example
   * {
   *   "$ranges": [
   *     { "$name": "slow", "$to": 10 },
   *     { "$name": "medium", "$from": 10, "$to": 50 },
   *     { "$name": "fast", "$from": 50 }
   *   ]
   * }
   */
  $ranges: RangeExpr[];
};

type RangeExpr = {
  /** Уникальное имя группы документов */
  $name: string;
  /** Нижняя граница значения поля (включая указанное значение) */
  $from?: number | string;
  /** Верхняя граница значения поля (не включая указанное значение) */
  $to?: number | string;
};

type PercentilesFacetExpr = {
  /**
   * Построение медианного значения или доверительных интервалов по заданному полю
   * @example
   * [50] // медиана
   * [5, 95] // доверительный интервал 5-95 %
   * [90, 99, 99.9, 99.99] // уровни доверия в процентах
   */
  $percentiles: number[];
};

/** Фасет по одному полю документа */
type FacetExpr =
  | "$interval"
  | "$samples"
  | SamplesFacetExpr
  | RangesFacetExpr
  | PercentilesFacetExpr;

/**
 * Набор агрегаций по различным полям для фасетного поиска
 * @example
 * {
 *   "Price": "$interval"
 *   "Tags.Title": { "$samples": 10 },
 *   "Parameters": {
 *     "Speed": {
 *       "$ranges": [
 *         { "$name": "slow", "$to": 10 },
 *         { "$name": "medium", "$from": 10, "$to": 50 },
 *         { "$name": "fast", "$from": 50 }
 *       ]
 *     }
 *   }
 * }
 */
type FacetsExpr<T> = T extends Primitive
  ? FacetExpr
  :
      | {
          [K in keyof T]?: FacetsExpr<UnwrapArr<T[K]>>;
        }
      | {
          [K in string]: FacetsExpr<any>;
        };

interface BaseRequest<T> {
  /**
   * Выбор полей документа. Поддерживает wildcards
   * @example
   * ["Id", "Title", "Regions.*"]
   */
  $select?: NonEmptyArr<keyof T | "_id" | "_index"> | NonEmptyArr<string>;
  /**
   * Настройка подсвеченных фрагментов по выбранным полям.
   * Если поля не указаны, используется псевдо-поле "_all",
   * включающее все полнотекстовые поля документа
   * @example
   * {
   *   "Title": { "$count": 1, "$length": 120 },
   *   "Body": { "$count": 2, "$length": 300 }
   * }
   */
  $snippets?: SnippetExpr | SnippetsExpr<T>;
  /**
   * Индекс(ы) для поиска
   * @example
   * "qp.news", "qp.*", ["dpc.tariffs", "dpc.services"]
   */
  $from: string | NonEmptyArr<string>;
  /**
   * Строка поискового запроса
   * @example
   * "тарифы и услуги"
   */
  $query?: string;
  /**
   * Минимальное количество найденных слов из @see $query,
   * при котором документ попадает в выдачу. По-умолчанию необходимы ВСЕ слова.
   * https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-minimum-should-match.html
   * @example
   * 3   — должны быть найдены не менее трех слова
   * -1  — должны быть найдены все слова кроме одного
   * "80%" — должны быть найдены не менее 80% слов
   */
  $requiredWordsCount?: number | string;
  /**
   * Веса полей для полнотекстового поиска
   * @example
   * { "Title": 5, "Body": 1, "Groups": { "Title": 2 } }
   */
  $weights?: WeightsExpr<T>;
  /**
   * Набор условий для фильтрации документов.
   * Условия внутри одного объекта объединяются через AND
   * @example
   * {
   *   "Region.Alias": ["moskva", "spb"],
   *   "PublishDate": { "$gt": "2015-01-01" }
   * }
   */
  $where?: WhereExpr<T & ElasticDocument>;
  /**
   * Набор условий для фильтрации контекстных полей документа.
   * При отсутствии берется из @see $where
   * Условия внутри одного объекта объединяются через AND
   * @example
   * {
   *   "Region.Alias": ["moskva", "spb"],
   *   "Groups.Title": "Новости Абонентам"
   * }
   */
  $context?: FilterExpr<T & ElasticDocument>;
  /**
   * Порядок сортировки результатов
   * @example
   * ["Region.Alias", { "Parameters.Price": "desc" }, "_score"]
   */
  $orderBy?: OrderByExpr<T & ElasticDocument> | NonEmptyArr<OrderByExpr<T & ElasticDocument>>;
  /**
   * Максимальное кол-во результатов в выдаче @default 50
   */
  $limit?: number;
}

/**
 * Запрос для полнотекстового поиска документов
 * с синонимами и морфологией, а также фасетного поиска
 */
export interface SearchRequest<T = any> extends BaseRequest<T> {
  /**
   * Пропуск указанного числа результатов перед выдачей
   */
  $offset?: number;
  /**
   * Условия исправления поискового запроса и результатов выдачи по исправленному запросу
   * @example
   * {
   *   "$query": { "$ifFoundLte": 10 },
   *   "$results": { "$ifFoundLte": 5 }
   * }
   */
  $correct?: CorrectExpr;
  /**
   * Набор агрегаций по различным полям для фасетного поиска
   * @example
   * {
   *   "Price": "$interval"
   *   "Tags.Title": { "$samples": 10 },
   *   "Parameters": {
   *     "Speed": {
   *       "$ranges": [
   *         { "$name": "slow", "$to": 10 },
   *         { "$name": "medium", "$from": 10, "$to": 50 },
   *         { "$name": "fast", "$from": 50 }
   *       ]
   *     }
   *   }
   * }
   */
  $facets?: FacetsExpr<T> & { _index?: "$samples" | SamplesFacetExpr };
}

/** Запрос для поиска документов по префиксам слов */
export interface SuggestRequest<T = any> extends BaseRequest<T> {
  /**
   * Строка поискового запроса
   * @example
   * "тарифы и услуги"
   */
  $query: string;
}

/** Запрос для поиска документов по префиксам слов */
export interface QueryCompletionRequest {
  /**
   * Строка поискового запроса
   * @example
   * "тарифы и услуги"
   */
  $query?: string;
  /**
 * Регион для фильтрации
 * @example
 * "spb"
 */
  $region?: string;
  /**
   * Максимальное кол-во результатов в выдаче @default 50
   */
  $limit?: number;
}

/** Запрос на дополнение строки поискового ввода */
export interface CompletionRequest<T = any> extends BaseRequest<T> {
  $select?: never;
  $snippets?: never;
  $requiredWordsCount?: never;
  $orderBy?: never;
  $context?: never;
  /**
   * Строка поискового запроса
   * @example
   * "тарифы и услуги"
   */
  $query: string;
  /**
   * Веса полей для автокомплита
   * @example
   * { "Title": 5, "Body": 1, "Groups": { "Title": 2 } }
   */
  $weights: WeightsExpr<T>;
  /**
   * Максимальное кол-во результатов в выдаче @default 10
   */
  $limit?: number;
}

/** Запрос для поиска документов по префиксам слов */
export interface QueryCompletionRegistrationRequest {
  /**
   * Строка поискового запроса
   * @example
   * "тарифы и услуги"
   */
  $query: string;
  /**
   * Регион для фильтрации
   * @example
   * "spb"
 */
  $region?: string;
}

/** Базовый интерфейс для результатов поиска */
export interface ElasticDocument {
  /** Строковый идентификатор документа в Elastic */
  _id: string;
  /** Сокращенное имя индекса, которому принадлежит документ */
  _index: string;
  /** Мера релевантности документа в рамках запроса */
  _score: number;
  /**
   * Словарь подсвеченных фрагментов текста, сгруппированных по имени поля
   * @example
   * {
   *   "Title": ["В <b>Москве</b> открыли"],
   *   "Regions.Title": ["<b>Москва</b> и область"]
   * }
   */
  _snippets?: Record<string, string[]>;
}

type SearchDocument<T> = T extends ElasticDocument ? T : T & ElasticDocument;

/** Результат поиска */
export interface SearchResponse<T> {
  /** Копия HTTP статус-кода в теле ответа */
  status: 200 | 400 | 404 | 408 | 502;
  /** Общее количество найденных документов */
  totalCount: number;
  /** Найденные документы */
  documents: SearchDocument<T>[];
  /** Результат исправления поисковой строки пользователя */
  queryCorrection?: {
    /** Исправленная поисковая строка в текстовом виде */
    text: string;
    /** Исправленная поисковая строка с HTML-выделением исправленных фраз */
    snippet: string;
    /** Было ли применено исправление при поиске результатов */
    resultsAreCorrected: boolean;
  };
  /** Словарь значений фасетов, сгруппированных по имени поля */
  facets?: Record<string, FacetItem>;
}

type SuggestDocument<T> = T extends ElasticDocument ? T : T & ElasticDocument;

/** Результат поиска документов по префиксам слов в текстах */
export interface SuggestResponse<T> {
  /** Копия HTTP статус-кода в теле ответа */
  status: 200 | 400 | 404 | 408 | 502;
  /** Найденные документы */
  documents: SuggestDocument<T>[];
}

/** Результат дополнения строки поискового ввода */
export interface CompletionResponse {
  /** Копия HTTP статус-кода в теле ответа */
  status: 200 | 400 | 404 | 408 | 502;
  /** Предлагаемые поисковые фразы */
  phrases: string[];
}

/** Результат дополнения строки поискового ввода */
export interface QueryCompletion {
  /** Предлагаемые поисковые фразы */
  key: string;
}

/**
 * @example
 * {
 *  "Price": [
 *    { "from": 100, "to": 1250 }
 *  ]
 * }
 */
export interface IntervalFacet<T extends string | number = any> {
  /** Минимальное значение в выборке */
  from: T;
  /** Максимальное значение в выборке */
  to: T;
}

/**
 * @example
 * {
 *   "Tags.Title": [
 *     { "value": "приложения", "count": 1200 },
 *     { "value": "игры", "count": 500 }
 *   ]
 * }
 */
export interface SamplesFacet<T extends string | number = any> {
  /** Значение поля */
  value: T;
  /** Количество документов с указанным значением поля */
  count: number;
}

/**
 * @example
 * {
 *   "Price": [
 *     { "percent": 99, "value": 1000 },
 *     { "percent": 99.9, "value": 1200 }
 *   ]
 * }
 */
export interface PercentileFacet<T extends string | number = any> {
  /** Одна из переданных квантилей в процентах */
  percent: number;
  /** Значение поля, которое соответствует указанной квантили */
  value: T;
}

/**
 * @example
 * {
 *   "Parameters.Speed": [
 *     { "name": "slow", "to": 10, "count": 5 },
 *     { "name": "slow", "from": 10, "to": 50, "count": 10 },
 *     { "name": "slow", "from": 50, "count": 5 },
 *   ]
 * }
 */
export interface RangeFacet<T extends string | number = any> {
  /** Название группы документов */
  name: string;
  /** Нижняя граница значения в группе документов */
  from?: T;
  /** Верхняя граница значения в группе документов */
  to?: T;
  /** Количество документов с указанным значением поля */
  count: number;
}

/** Фасет по одному полю */
export interface FacetItem {
  /** Границы значения поля в выборке */
  interval?: IntervalFacet;
  /** Часто встречающиеся значения поля в выборке */
  samples?: SamplesFacet[];
  /** Статистическое распределение значений поля */
  percentiles?: PercentileFacet[];
  /** Распределение количества документов по каждому указанному интервалу */
  ranges?: RangeFacet[];
}
