CREATE TABLE CostEvent 
(
	Category VARCHAR(256),
	MeterId VARCHAR(56),
	Price FLOAT,
	Cost FLOAT,
	Unit VARCHAR(28),
	Quantity FLOAT,
	MeterName VARCHAR(256),
	StartTime DATETIME,
	EndTime DATETIME,
	SubscriptionId VARCHAR(80),
	ResourceGroup VARCHAR(128),
	ReportedStartDate DATETIME,
	ReportedEndDate DATETIME,
	ResourceProvider VARCHAR(512),
	ResourceName VARCHAR(128),
	ResourceInstanceName VARCHAR(128),
	Tags VARCHAR(512),
	[Location] VARCHAR(128)
)

alter table CostEvent add PartitionId int 
alter table CostEvent add EventProcessedUtcTime DateTime
alter table CostEvent add EventEnqueuedUtcTime DateTime

CREATE TABLE MetricEvent 
(
	Average float,
	Maximum float,
	Minimum FLOAT,
	Total FLOAT,
	DatabaseName VARCHAR(56),
	ServerName varchar(256),
	MetricName VARCHAR(128),
    ServiceType varchar(56),
	StartTime DATETIME,
	EndTime DATETIME,
	PartitionId int, 
    EventProcessedUtcTime DateTime,
    EventEnqueuedUtcTime DateTime
)