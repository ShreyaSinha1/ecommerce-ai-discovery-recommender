# ecommerce-ai-discovery-recommender

  +---------------------------------------------------------------------------------+

  |                             PRESENTATION LAYER (SPA)                            |
  |  [Angular Core] ──► [NgRx Store] ──► [Component UI] ──► [RxJS Search Stream]    |
  +---------------------------------------+-----------------------------------------+
                                          │ HTTPS REST (JSON over JWT)
                                          ▼
  +---------------------------------------------------------------------------------+

  |                           EDGE REST API GATEWAY HOST                            |
  |  [Rate Limiting] ──► [Exception Middleware] ──► [Routing: Discovery/Recs]      |
  +---------------------------------------+-----------------------------------------+
                                          │ MediatR CQRS Pipelines
                                          ▼
  +---------------------------------------------------------------------------------+

  |                        APPLICATION CORE (ORCHESTRATION)                         |
  |  [SemanticSearchQuery]   [GetPersonalizedRecsQuery]   [ProductIngestionCommand] |
  +--------------------+------------------+-----------------------+-----------------+
                       │                  │                       │
                       │ HTTP/gRPC        │ EF Core 9             │ Cloud Events
                       ▼                  ▼                       ▼
  +------------------------+  +-----------------------+  +--------------------------+

  | EXTERNAL COGNITIVE APP |  |    RELATIONAL DATA    |  | EVENT STREAMING LAYER    |
  | [Azure OpenAI Service] |  |   DATA ACCESS LAYER   |  | [Azure Service Bus]      |
  |  text-embedding-3-small|  | [ApplicationDbContext]|  |  Catalog Sync Events     |
  +-------------------+----+  +-----------+-----------+  +------------+-------------+
                      │                   │                           │
                      │ float[] Vector    │ SQL + Vector Operators    │ Asynchronous Change
                      ▼                   ▼                           ▼
  +---------------------------------------------------------------------------------+

  |                    DATA STORAGE & VECTOR ENGINE PIPELINES                       |
  |  +---------------------------------------------------------------------------+  |
  |  |                 [Azure Database for PostgreSQL Flexible Server]            |  |
  |  |  * Core Tables: Products, Orders, Users, Inventories                      |  |
  |  |  * Vector Store: product_embeddings 表 Map Type: vector(1536)              |  |
  |  |  * Performance Index Struct: HNSW (Hierarchical Navigable Small World)    |  |












  [1. THE REAL-TIME USER DISCOVERY LOOP]
User Intent String ──► WebAPI Controller ──► Azure OpenAI ──► 1536-dim Vector ──► pgvector HNSW Query ──► Client JSON

[2. THE BACKGROUND CATALOG SYNC LOOP]
Catalog DB Write ──► Azure Service Bus Event ──► Worker Process ──► Azure OpenAI ──► pgvector INSERT/UPDATE Upsert

  |  +---------------------------------------------------------------------------+  |
  +---------------------------------------------------------------------------------+
