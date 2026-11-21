-- Sample data for the IpInformation database, so it is not empty before the application has been used.
-- Run it manually (e.g. in SSMS) after the migrations have been applied. The application never runs it.
--
-- 9 countries and 28 IP addresses (4 for Greece, 3 for each other country), mirroring the sample report.
-- Country names and codes are exactly what IP2C returned for these IPs on 2026-11-21.
--
-- Safe to run more than once, and on a database that already has data: it only inserts the countries
-- (by TwoLetterCode) and IPs (by IP) that are missing. Ids come from IDENTITY; CreatedAt/UpdatedAt from the column defaults.

USE [IpInformation];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

INSERT INTO dbo.Countries (Name, TwoLetterCode, ThreeLetterCode)
SELECT s.Name, s.TwoLetterCode, s.ThreeLetterCode
FROM (VALUES
    (N'Greece',                         'GR', 'GRC'),
    (N'Italy',                          'IT', 'ITA'),
    (N'Japan',                          'JP', 'JPN'),
    (N'Spain',                          'ES', 'ESP'),
    (N'United States of America (the)', 'US', 'USA'),
    (N'China',                          'CN', 'CHN'),
    (N'Cyprus',                         'CY', 'CYP'),
    (N'France',                         'FR', 'FRA'),
    (N'Germany',                        'DE', 'DEU')
) AS s (Name, TwoLetterCode, ThreeLetterCode)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Countries AS c WHERE c.TwoLetterCode = s.TwoLetterCode);

INSERT INTO dbo.IpAddresses (IP, CountryId)
SELECT s.IP, c.Id
FROM (VALUES
    ('195.251.255.1',   'GR'), ('62.103.0.1',     'GR'), ('194.219.227.2', 'GR'), ('213.249.39.1', 'GR'),
    ('151.1.1.1',       'IT'), ('193.204.114.233','IT'), ('2.228.0.1',     'IT'),
    ('133.11.0.1',      'JP'), ('210.130.1.1',    'JP'), ('202.12.27.33',  'JP'),
    ('80.58.61.250',    'ES'), ('193.146.141.1',  'ES'), ('212.106.200.1', 'ES'),
    ('8.8.8.8',         'US'), ('4.2.2.2',        'US'), ('208.67.222.222','US'),
    ('114.114.114.114', 'CN'), ('223.5.5.5',      'CN'), ('180.76.76.76',  'CN'),
    ('212.31.96.1',     'CY'), ('194.42.0.1',     'CY'), ('82.102.64.1',   'CY'),
    ('80.67.169.12',    'FR'), ('212.27.40.240',  'FR'), ('193.51.24.1',   'FR'),
    ('194.25.2.129',    'DE'), ('85.214.20.141',  'DE'), ('212.227.0.1',   'DE')
) AS s (IP, TwoLetterCode)
JOIN dbo.Countries AS c ON c.TwoLetterCode = s.TwoLetterCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.IpAddresses AS i WHERE i.IP = s.IP);

COMMIT TRANSACTION;

-- Check: the same report the API returns.
SELECT c.Name AS CountryName, COUNT(*) AS AddressesCount, MAX(i.UpdatedAt) AS LastAddressUpdated
FROM dbo.IpAddresses AS i
JOIN dbo.Countries AS c ON c.Id = i.CountryId
GROUP BY c.Id, c.Name
ORDER BY c.Name;
