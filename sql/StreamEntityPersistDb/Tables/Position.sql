/*
The database must have a MEMORY_OPTIMIZED_DATA filegroup
before the memory optimized object can be created.

The bucket count should be set to about two times the 
maximum expected number of distinct values in the 
index key, rounded up to the nearest power of two.
*/

CREATE TABLE [dbo].[Position]
(
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED HASH WITH (BUCKET_COUNT = 131072), 
    [InstrumentId] INT NOT NULL, 
    [PortfolioId] INT NOT NULL, 
    [Volume] INT NOT NULL, 
    [Price] FLOAT NOT NULL, 
    [Timestamp] DATETIME NOT NULL
) WITH (MEMORY_OPTIMIZED = ON)