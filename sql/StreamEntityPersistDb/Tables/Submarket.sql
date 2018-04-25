/*
The database must have a MEMORY_OPTIMIZED_DATA filegroup
before the memory optimized object can be created.

The bucket count should be set to about two times the 
maximum expected number of distinct values in the 
index key, rounded up to the nearest power of two.
*/

CREATE TABLE [dbo].[Submarket]
(
	[Id] INT NOT NULL PRIMARY KEY NONCLUSTERED HASH WITH (BUCKET_COUNT = 131072), 
    [MarketId] INT NOT NULL, 
    [Name] NVARCHAR(50) NOT NULL, 
    [Timestamp] DATETIME NOT NULL
) WITH (MEMORY_OPTIMIZED = ON)