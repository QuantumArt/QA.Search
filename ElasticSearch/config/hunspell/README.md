https://github.com/elastic/hunspell

```json
"settings": {
  "analysis": {
    "filter": {
      "english_hunspell": {
        "type": "hunspell",
        "locale": "en_GB",
        "dedup": true
      },
      "russian_hunspell": {
        "type": "hunspell",
        "locale": "ru_RU",
        "dedup": true
      }
    }
  }
}
```