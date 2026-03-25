---

title: SQL Notes
description: Notes on SQL scripts for creation of triggers and stored procedures.
date: 03/24/2026

---

# SQL Notes

### SQL scripts for creation of triggers and stored procedures

* This project uses preview versions of SQL Server and preview features that must be enabled in each database that interacts with this application.

-Create external model within SQL:

```text
CREATE EXTERNAL MODEL MyOllamaModel
WITH (
      LOCATION = 'https://localhost:11434/api/embed',  <-- Ollama API endpoint for embeddings I was unable to configure HTTPS in Ollama
      API_FORMAT = 'Ollama',
      MODEL_TYPE = EMBEDDINGS,
      MODEL = 'all-minilm'
);
```

Now you can use this model in your SQL queries to generate embeddings for your data. For example:

SELECT AI_GENERATE_EMBEDDINGS(N'Test Text' USE MODEL MyOllamaModel);

The actual database schema is up to the developer and is not included in this project. I have included some scripts for special preview features using AI.

* [Create a trigger that uses AI to generate embeddings for a column in a table](./sql/Create_tr_generate_embeddings.sql)
* [Create a trigger that uses AI to generate a summary for a column in a table](./sql/Create_tr_generate_summary.sql)
* [Create a trigger that uses AI to generates keywords for a column in a table](./sql/Create_tr_generate_keywords.sql)
*