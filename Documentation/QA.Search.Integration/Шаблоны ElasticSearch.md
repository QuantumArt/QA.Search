# Шаблоны ElasticSearch

Для корректной и быстрой работы поиска с индексами ElasticSearch требуется эти индексы правильным образом сконфигурировать.  
ElasticSearch позволяет указывать настройки индекса при его создании, а также использование шаблонов индексации.  
Если создание индекса с заданными настройками выгодно использовать если структура индекса часто меняется, то для нас гораздо более предпочтительным является способ предварительной конфигурации настроек индекса через шаблоны.  
Шаблон задаётся один раз перед созданием индекса и содержит в себе паттерн имени индексов, к которым он будет применяться. Например если мы укажем паттерн `index.search.demosite.*`, то все индексы с разными именами, но фиксированной частью имени `index.search.demosite.` будут создаваться на основе этого шаблона.  
Это даёт нам гибкие возможности в настройке индексов. Можно настраивать как каждый индекс индивидуально, так и задать общую настройку индексации для всего проекта.  
Мы рассмотрим вариант с общей настройкой.  

## Шаблон для проекта Demosite

Ниже находится пример шаблона для индексов проекта Demosite.  
В последующих разделах мы рассмотрим его части более подробно.  

<details>
 <summary>Развернуть</summary>
```JSON
{
    "index_patterns":
    [
        "index.search.demosite.*"
    ],
    "template":
    {
        "settings":
        {
            "index":
            {
                "mapping":
                {
                    "total_fields":
                    {
                        "limit": "100000"
                    }
                },
                "analysis":
                {
                    "filter":
                    {
                        "stemmer_english":
                        {
                            "type": "stemmer",
                            "language": "english"
                        },
                        "synonyms_index":
                        {
                            "type": "synonym_graph",
                            "synonyms":
                            []
                        },
                        "stopwords_russian":
                        {
                            "type": "stop",
                            "stopwords": "_russian_"
                        },
                        "big_shingles":
                        {
                            "max_shingle_size": "4",
                            "min_shingle_size": "2",
                            "type": "shingle"
                        },
                        "shingles":
                        {
                            "max_shingle_size": "3",
                            "min_shingle_size": "2",
                            "type": "shingle"
                        },
                        "stopwords_english":
                        {
                            "type": "stop",
                            "stopwords": "_english_"
                        },
                        "synonyms_search":
                        {
                            "type": "synonym_graph",
                            "synonyms":
                            []
                        },
                        "stemmer_russian":
                        {
                            "type": "stemmer",
                            "language": "russian"
                        },
                        "hunspell_russian":
                        {
                            "locale": "ru",
                            "type": "hunspell",
                            "dedup": "true"
                        },
                        "edge_ngram":
                        {
                            "type": "edge_ngram",
                            "min_gram": "2",
                            "max_gram": "20"
                        },
                        "hunspell_english":
                        {
                            "locale": "en_GB",
                            "type": "hunspell",
                            "dedup": "true"
                        }
                    },
                    "char_filter":
                    {
                        "collapse_white_space":
                        {
                            "pattern": "[\\p{Z}\\r\\n]+",
                            "type": "pattern_replace",
                            "replacement": " "
                        },
                        "truncate_keyword":
                        {
                            "pattern": "^\\p{Z}*(.{2045}).*$",
                            "type": "pattern_replace",
                            "replacement": "$1..."
                        }
                    },
                    "normalizer":
                    {
                        "normalizer_keyword":
                        {
                            "type": "custom",
                            "char_filter":
                            [
                                "collapse_white_space",
                                "truncate_keyword"
                            ]
                        }
                    },
                    "analyzer":
                    {
                        "analyzer_shingles":
                        {
                            "filter":
                            [
                                "lowercase",
                                "stopwords_russian",
                                "stopwords_english",
                                "hunspell_russian",
                                "hunspell_english",
                                "shingles"
                            ],
                            "char_filter":
                            [
                                "html_strip"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        },
                        "analyzer_text":
                        {
                            "filter":
                            [
                                "lowercase",
                                "stopwords_russian",
                                "stopwords_english",
                                "hunspell_russian",
                                "hunspell_english"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        },
                        "analyzer_prefixes":
                        {
                            "filter":
                            [
                                "lowercase",
                                "stopwords_russian",
                                "stopwords_english",
                                "hunspell_russian",
                                "hunspell_english",
                                "edge_ngram"
                            ],
                            "char_filter":
                            [
                                "html_strip"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        },
                        "analyzer_regex":
                        {
                            "filter":
                            [
                                "lowercase",
                                "stopwords_english",
                                "stopwords_russian",
                                "stemmer_english",
                                "stemmer_russian"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        },
                        "analyzer_phrases":
                        {
                            "filter":
                            [
                                "lowercase",
                                "big_shingles"
                            ],
                            "char_filter":
                            [
                                "html_strip"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        },
                        "analyzer_synonyms_index":
                        {
                            "filter":
                            [
                                "lowercase",
                                "stopwords_russian",
                                "stopwords_english",
                                "hunspell_russian",
                                "hunspell_english",
                                "synonyms_index"
                            ],
                            "char_filter":
                            [
                                "html_strip"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        },
                        "analyzer_synonyms_search":
                        {
                            "filter":
                            [
                                "lowercase",
                                "stopwords_russian",
                                "stopwords_english",
                                "hunspell_russian",
                                "hunspell_english",
                                "synonyms_search"
                            ],
                            "type": "custom",
                            "tokenizer": "unicode_words"
                        }
                    },
                    "tokenizer":
                    {
                        "unicode_words":
                        {
                            "pattern": "\\d+[\\.,]\\d+|[\\p{L}\\d_+*#]+",
                            "type": "pattern",
                            "group": "0"
                        }
                    }
                },
                "number_of_shards": "1",
                "number_of_replicas": "2"
            }
        },
        "mappings":
        {
            "dynamic_date_formats":
            [
                "MM/dd/yyyy",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy HH:mm:ss",
                "dd.MM.yyyy",
                "dd.MM.yyyy HH:mm",
                "dd.MM.yyyy HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy-MM-dd'T'HH:mm",
                "yyyy-MM-dd'T'HH:mm:ss",
                "yyyy-MM-dd'T'HH:mm:ssZZZZZ",
                "yyyy-MM-dd'T'HH:mm:ss.SSS",
                "yyyy-MM-dd'T'HH:mm:ss.SSSZZZZZ"
            ],
            "dynamic_templates":
            [
                {
                    "text":
                    {
                        "match_pattern": "regex",
                        "path_match": "title|text",
                        "mapping":
                        {
                            "copy_to":
                            [
                                "_phrases",
                                "_prefixes",
                                "_shingles",
                                "_synonyms"
                            ],
                            "normalizer": "normalizer_keyword",
                            "fields":
                            {
                                "prefixes":
                                {
                                    "search_analyzer": "analyzer_text",
                                    "analyzer": "analyzer_prefixes",
                                    "type": "text"
                                },
                                "synonyms":
                                {
                                    "search_analyzer": "analyzer_synonyms_search",
                                    "analyzer": "analyzer_synonyms_index",
                                    "type": "text"
                                },
                                "shingles":
                                {
                                    "analyzer": "analyzer_shingles",
                                    "type": "text"
                                },
                                "phrases":
                                {
                                    "analyzer": "analyzer_phrases",
                                    "type": "text"
                                }
                            },
                            "type": "keyword"
                        }
                    }
                },
                {
                    "nested":
                    {
                        "match_pattern": "regex",
                        "mapping":
                        {
                            "type": "nested"
                        },
                        "match_mapping_type": "object",
                        "match": "Children"
                    }
                },
                {
                    "number":
                    {
                        "match_mapping_type": "long",
                        "mapping":
                        {
                            "type": "double"
                        }
                    }
                },
                {
                    "string":
                    {
                        "match_mapping_type": "string",
                        "mapping":
                        {
                            "type": "keyword",
                            "normalizer": "normalizer_keyword"
                        }
                    }
                }
            ],
            "properties":
            {
                "_phrases":
                {
                    "type": "text",
                    "analyzer": "analyzer_phrases"
                },
                "_prefixes":
                {
                    "type": "text",
                    "analyzer": "analyzer_prefixes",
                    "search_analyzer": "analyzer_text",
                    "store": true
                },
                "_shingles":
                {
                    "type": "text",
                    "analyzer": "analyzer_shingles",
                    "store": true
                },
                "_synonyms":
                {
                    "type": "text",
                    "analyzer": "analyzer_synonyms_index",
                    "search_analyzer": "analyzer_synonyms_search"
                },
                "contentitemid":
                {
                    "type": "double"
                },
                "SearchUrl":
                {
                    "properties":
                    {
                        "SearchUrl":
                        {
                            "normalizer": "normalizer_keyword",
                            "type": "keyword"
                        }
                    }
                },
                "title":
                {
                    "copy_to":
                    [
                        "_phrases",
                        "_prefixes",
                        "_shingles",
                        "_synonyms"
                    ],
                    "normalizer": "normalizer_keyword",
                    "type": "keyword",
                    "fields":
                    {
                        "prefixes":
                        {
                            "search_analyzer": "analyzer_text",
                            "analyzer": "analyzer_prefixes",
                            "type": "text"
                        },
                        "synonyms":
                        {
                            "search_analyzer": "analyzer_synonyms_search",
                            "analyzer": "analyzer_synonyms_index",
                            "type": "text"
                        },
                        "shingles":
                        {
                            "analyzer": "analyzer_shingles",
                            "type": "text"
                        },
                        "phrases":
                        {
                            "analyzer": "analyzer_phrases",
                            "type": "text"
                        }
                    }
                },
                "text":
                {
                    "copy_to":
                    [
                        "_phrases",
                        "_prefixes",
                        "_shingles",
                        "_synonyms"
                    ],
                    "normalizer": "normalizer_keyword",
                    "type": "keyword",
                    "fields":
                    {
                        "prefixes":
                        {
                            "search_analyzer": "analyzer_text",
                            "analyzer": "analyzer_prefixes",
                            "type": "text"
                        },
                        "synonyms":
                        {
                            "search_analyzer": "analyzer_synonyms_search",
                            "analyzer": "analyzer_synonyms_index",
                            "type": "text"
                        },
                        "shingles":
                        {
                            "analyzer": "analyzer_shingles",
                            "type": "text"
                        },
                        "phrases":
                        {
                            "analyzer": "analyzer_phrases",
                            "type": "text"
                        }
                    }
                }
            }
        }
    }
}
```
</details>

## Словари морфологии

Для начала стоит обратить внимание на поддержку морфологии.  
Если в рамках проекта требуется в поиске учитывать семантику, синонимы и прочие особенности слова как единицы языка - требуется подключить словари hunspell в сам ElasticSearch.  

### Подключение словаря

Словари можно найти в репозитории https://github.com/elastic/hunspell/tree/master/dicts  
В нашем случае мы будем использовать словари для русского, английского и белорусского языков (ru,en_GB,be_BY).  
Скачиваем соответствующие директории с репозитория и помещаем их на сервере, где установлен ElasticSearch в директорию `/etc/elasticsearch/hunspell/`.

### Добавление фильтров словарей

Для начала использования словаря требуется создать фильтр с подключенным словарём, для чего добавим в `filters` следующий блок (пример для русского языка):  

```JSON
"hunspell_russian":
{
    "locale": "ru",
    "type": "hunspell",
    "dedup": "true"
}
```

Где `locale` должно соответствовать названию директории, содержащей файлы словаря, `type` следует указать `hunspell` (это укажет ElasticSearch что это подключаемый словарь, наличие словаря будет проверено в момент создания шаблона), `dedup` указывает на необходимость удаления дубликатов при загрузке из словаря (если они там есть).  

### Использование фильтров словарей  

Для использования словаря в анализаторах требуется указать название фильтра с нужным словарём в списке фильтров анализатора, например таким образом:

```JSON
"analyzer_text":
    {
    "filter":
    [
        "lowercase",
        "stopwords_russian",
        "stopwords_english",
        "hunspell_russian",
        "hunspell_english"
    ],
    "type": "custom",
    "tokenizer": "unicode_words"
}
```

## Ключевые для работы поиска настройки шаблона

Для корректной работы поиск требует, как минимум следующих обязательных настроек.  

### Паттерн разбора по регулярному выражению

Для разбора текста на отдельные блоки, по которым будет осуществляться поиск, выполняется разбор по регулярному выражению, которое настраивается в этом блоке:

```JSON
"unicode_words":
{
    "pattern": "\\d+[\\.,]\\d+|[\\p{L}\\d_+*#]+",
    "type": "pattern",
    "group": "0"
}
```

### Анализаторы текста

Для корректной работы с текстом в шаблоне должны быть заданы анализаторы для работы с `text`, `synonyms`, `shingles`, `prefixes` и `regex`, все они указаны в секции `analyzer`.  
Стоит отметить, что каждый анализатор имеет свой набор зависимостей в виде фильтров, токенов и прочего. Все зависимости приведены в этом же файле шаблона в соответствующих блоках.  

## Динамические привязки

Для настройки общих привязок к ряду полей индекса в автоматическом режиме используются динамические привязки в разделе `dynamic_templates`.  

### Разбор текстовых полей  

При анализе текстовых полей динамической привязкой происходит нормализация и разбор текста на `prefixes`, `synonyms`, `shingles` и `phrases` с копированием их в соответствующие поля индекса.  
Всё это выполняется следующей настройкой:  

```JSON
"text":
{
    "match_pattern": "regex",
    "path_match": "title|text",
    "mapping":
    {
        "copy_to":
        [
            "_phrases",
            "_prefixes",
            "_shingles",
            "_synonyms"
        ],
        "normalizer": "normalizer_keyword",
        "fields":
        {
            "prefixes":
            {
                "search_analyzer": "analyzer_text",
                "analyzer": "analyzer_prefixes",
                "type": "text"
            },
            "synonyms":
            {
                "search_analyzer": "analyzer_synonyms_search",
                "analyzer": "analyzer_synonyms_index",
                "type": "text"
            },
            "shingles":
            {
                "analyzer": "analyzer_shingles",
                "type": "text"
            },
            "phrases":
            {
                "analyzer": "analyzer_phrases",
                "type": "text"
            }
        },
        "type": "keyword"
    }
}
```

Тут стоит обратить внимание на поле `path_match`, в нём задаётся список полей, которые будут попадать под анализ. Здесь следует задать актуальный для проекта список полей.  

### Дополнительные динамические привязки

Помимо привязки для разбора текстовых полей регулярным выражением описанной выше можно задавать дополнительные привязки, например правила для разбора чисел и так далее.  

## Настройка индексации конкретного поля  

Если мы хотим, чтобы поле при индексации анализировалось и хранилось особым образом, то для него стоит создать правило в секции `properties`.  
Правила для `_phrases`, `_prefixes`, `_shingles`, `_synonyms`, `contentitemid` и `SearchUrl` должны соответствовать тем, что указаны в примере шаблона, они обеспечивают работу базовой функциональности Api поиска.  
Идущие далее правила можно настраивать под задачи проекта, а также можно добавлять новые правила.  

### Индексация текстовых полей

Для наиболее эффективной индексации текстовых полей в связке с Api поиска следует использовать следующий шаблон индексации:

```JSON
"title":
{
    "copy_to":
    [
        "_phrases",
        "_prefixes",
        "_shingles",
        "_synonyms"
    ],
    "normalizer": "normalizer_keyword",
    "type": "keyword",
    "fields":
    {
        "prefixes":
        {
            "search_analyzer": "analyzer_text",
            "analyzer": "analyzer_prefixes",
            "type": "text"
        },
        "synonyms":
        {
            "search_analyzer": "analyzer_synonyms_search",
            "analyzer": "analyzer_synonyms_index",
            "type": "text"
        },
        "shingles":
        {
            "analyzer": "analyzer_shingles",
            "type": "text"
        },
        "phrases":
        {
            "analyzer": "analyzer_phrases",
            "type": "text"
        }
    }
}
```

Где `title` - название столбца для индексации, а далее стандартные правила наиболее эффективной индексации для Api поиска. Правила можно дополнять и расширять, но крайне не рекомендуется урезать или убирать.  
При отсутствии корректно настроенных правил индексации работоспособность поиска не гарантируется.  

## Применение индекса  

Для применения индекса в ElasticSearch требуется выполнить `PUT` запрос в ElasticSearch в Endpoint `_index_template` и через `/` указать название шаблона, например `http://elastic:9200/_index_template/search.demosite`.  
Выполнять запрос можно через `curl`, `Insomnia`, `PostMan` или любые другие альтернативы.  
Пример запроса для `curl`:  

<details>
 <summary>Развернуть</summary>
```
curl --request PUT \
  --url http://127.0.0.1:9200/_index_template/search.demosite \
  --header 'Content-Type: application/json' \
  --data '{
	"index_patterns": [
		"index.search.demosite.*"
	],
	"template": {
		"settings": {
			"index": {
				"mapping": {
					"total_fields": {
						"limit": "100000"
					}
				},
				"analysis": {
					"filter": {
						"stemmer_english": {
							"type": "stemmer",
							"language": "english"
						},
						"synonyms_index": {
							"type": "synonym_graph",
							"synonyms": []
						},
						"stopwords_russian": {
							"type": "stop",
							"stopwords": "_russian_"
						},
						"big_shingles": {
							"max_shingle_size": "4",
							"min_shingle_size": "2",
							"type": "shingle"
						},
						"shingles": {
							"max_shingle_size": "3",
							"min_shingle_size": "2",
							"type": "shingle"
						},
						"stopwords_english": {
							"type": "stop",
							"stopwords": "_english_"
						},
						"synonyms_search": {
							"type": "synonym_graph",
							"synonyms": []
						},
						"stemmer_russian": {
							"type": "stemmer",
							"language": "russian"
						},
						"hunspell_russian": {
							"locale": "ru",
							"type": "hunspell",
							"dedup": "true"
						},
						"edge_ngram": {
							"type": "edge_ngram",
							"min_gram": "2",
							"max_gram": "20"
						},
						"hunspell_english": {
							"locale": "en_GB",
							"type": "hunspell",
							"dedup": "true"
						}
					},
					"char_filter": {
						"collapse_white_space": {
							"pattern": "[\\p{Z}\\r\\n]+",
							"type": "pattern_replace",
							"replacement": " "
						},
						"truncate_keyword": {
							"pattern": "^\\p{Z}*(.{2045}).*$",
							"type": "pattern_replace",
							"replacement": "$1..."
						}
					},
					"normalizer": {
						"normalizer_keyword": {
							"type": "custom",
							"char_filter": [
								"collapse_white_space",
								"truncate_keyword"
							]
						}
					},
					"analyzer": {
						"analyzer_shingles": {
							"filter": [
								"lowercase",
								"stopwords_russian",
								"stopwords_english",
								"hunspell_russian",
								"hunspell_english",
								"shingles"
							],
							"char_filter": [
								"html_strip"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						},
						"analyzer_text": {
							"filter": [
								"lowercase",
								"stopwords_russian",
								"stopwords_english",
								"hunspell_russian",
								"hunspell_english"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						},
						"analyzer_prefixes": {
							"filter": [
								"lowercase",
								"stopwords_russian",
								"stopwords_english",
								"hunspell_russian",
								"hunspell_english",
								"edge_ngram"
							],
							"char_filter": [
								"html_strip"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						},
						"analyzer_regex": {
							"filter": [
								"lowercase",
								"stopwords_english",
								"stopwords_russian",
								"stemmer_english",
								"stemmer_russian"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						},
						"analyzer_phrases": {
							"filter": [
								"lowercase",
								"big_shingles"
							],
							"char_filter": [
								"html_strip"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						},
						"analyzer_synonyms_index": {
							"filter": [
								"lowercase",
								"stopwords_russian",
								"stopwords_english",
								"hunspell_russian",
								"hunspell_english",
								"synonyms_index"
							],
							"char_filter": [
								"html_strip"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						},
						"analyzer_synonyms_search": {
							"filter": [
								"lowercase",
								"stopwords_russian",
								"stopwords_english",
								"hunspell_russian",
								"hunspell_english",
								"synonyms_search"
							],
							"type": "custom",
							"tokenizer": "unicode_words"
						}
					},
					"tokenizer": {
						"unicode_words": {
							"pattern": "\\d+[\\.,]\\d+|[\\p{L}\\d_+*#]+",
							"type": "pattern",
							"group": "0"
						}
					}
				},
				"number_of_shards": "1",
				"number_of_replicas": "2"
			}
		},
		"mappings": {
			"dynamic_date_formats": [
				"MM/dd/yyyy",
				"MM/dd/yyyy HH:mm",
				"MM/dd/yyyy HH:mm:ss",
				"dd.MM.yyyy",
				"dd.MM.yyyy HH:mm",
				"dd.MM.yyyy HH:mm:ss",
				"yyyy-MM-dd",
				"yyyy-MM-dd'\''T'\''HH:mm",
				"yyyy-MM-dd'\''T'\''HH:mm:ss",
				"yyyy-MM-dd'\''T'\''HH:mm:ssZ",
				"yyyy-MM-dd'\''T'\''HH:mm:ss.SSS",
				"yyyy-MM-dd'\''T'\''HH:mm:ss.SSSZ"
			],
			"dynamic_templates": [
				{
					"text": {
						"match_pattern": "regex",
						"path_match": "title|text",
						"mapping": {
							"copy_to": [
								"_phrases",
								"_prefixes",
								"_shingles",
								"_synonyms"
							],
							"normalizer": "normalizer_keyword",
							"fields": {
								"prefixes": {
									"search_analyzer": "analyzer_text",
									"analyzer": "analyzer_prefixes",
									"type": "text"
								},
								"synonyms": {
									"search_analyzer": "analyzer_synonyms_search",
									"analyzer": "analyzer_synonyms_index",
									"type": "text"
								},
								"shingles": {
									"analyzer": "analyzer_shingles",
									"type": "text"
								},
								"phrases": {
									"analyzer": "analyzer_phrases",
									"type": "text"
								}
							},
							"type": "keyword"
						}
					}
				},
				{
					"nested": {
						"match_pattern": "regex",
						"mapping": {
							"type": "nested"
						},
						"match_mapping_type": "object",
						"match": "Children"
					}
				},
				{
					"number": {
						"match_mapping_type": "long",
						"mapping": {
							"type": "double"
						}
					}
				},
				{
					"string": {
						"match_mapping_type": "string",
						"mapping": {
							"type": "keyword",
							"normalizer": "normalizer_keyword"
						}
					}
				}
			],
			"properties": {
				"_phrases": {
					"type": "text",
					"analyzer": "analyzer_phrases"
				},
				"_prefixes": {
					"type": "text",
					"analyzer": "analyzer_prefixes",
					"search_analyzer": "analyzer_text",
					"store": true
				},
				"_shingles": {
					"type": "text",
					"analyzer": "analyzer_shingles",
					"store": true
				},
				"_synonyms": {
					"type": "text",
					"analyzer": "analyzer_synonyms_index",
					"search_analyzer": "analyzer_synonyms_search"
				},
				"contentitemid": {
					"type": "double"
				},
				"SearchUrl": {
					"properties": {
						"SearchUrl": {
							"normalizer": "normalizer_keyword",
							"type": "keyword"
						}
					}
				},
				"title": {
					"copy_to": [
						"_phrases",
						"_prefixes",
						"_shingles",
						"_synonyms"
					],
					"normalizer": "normalizer_keyword",
					"type": "keyword",
					"fields": {
						"prefixes": {
							"search_analyzer": "analyzer_text",
							"analyzer": "analyzer_prefixes",
							"type": "text"
						},
						"synonyms": {
							"search_analyzer": "analyzer_synonyms_search",
							"analyzer": "analyzer_synonyms_index",
							"type": "text"
						},
						"shingles": {
							"analyzer": "analyzer_shingles",
							"type": "text"
						},
						"phrases": {
							"analyzer": "analyzer_phrases",
							"type": "text"
						}
					}
				},
				"text": {
					"copy_to": [
						"_phrases",
						"_prefixes",
						"_shingles",
						"_synonyms"
					],
					"normalizer": "normalizer_keyword",
					"type": "keyword",
					"fields": {
						"prefixes": {
							"search_analyzer": "analyzer_text",
							"analyzer": "analyzer_prefixes",
							"type": "text"
						},
						"synonyms": {
							"search_analyzer": "analyzer_synonyms_search",
							"analyzer": "analyzer_synonyms_index",
							"type": "text"
						},
						"shingles": {
							"analyzer": "analyzer_shingles",
							"type": "text"
						},
						"phrases": {
							"analyzer": "analyzer_phrases",
							"type": "text"
						}
					}
				}
			}
		}
	}
}'
```
</details>

## Обновление шаблона

Для обновления шаблона нужно поправить текст шаблона, после чего выполнить повторную отправку шаблона тем же методом, что указан в блоке `Применение индекса`.  
Стоит помнить, что уже созданные индексы не будут обновлены и данные в них не будут переиндексированы после обновления шаблона.  
Для применения шаблона к индексам следует дождаться плановой полной переиндексации сервисом индексации контента, либо запустить переиндексацию вручную через консоль администратора поиска `QP.Search.Admin`.
