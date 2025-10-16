select COUNT(*) from tweets_sentiments 
WHERE sentiment IS NOT NULL;

"count"
10802300
------------------------------------------------------------------------
SELECT * FROM tweets_sentiments 
WHERE sentiment <> ''
ORDER BY inserted_at ASC
LIMIT 100;

------------------------------------------------------------------------
SELECT COUNT(*),MAX(inserted_at),MIN(inserted_at) FROM tweets_sentiments 
WHERE sentiment <> '';

"count"	"max"	"min"
4382504	"2025-08-10 12:30:00"	"2023-02-07 14:55:20"
------------------------------------------------------------------------
SELECT COUNT(*),MAX(inserted_at),MIN(inserted_at),MAX(inserted_at)-MIN(inserted_at) FROM tweets_users

"count"	"max"	"min"
11920770	"2025-08-10 12:30:00"	"2023-02-07 14:55:20" "915 days 00:59:31"
------------------------------------------------------------------------

SELECT 
	COUNT(*)filter (where sentiment='positive') as pos,
	COUNT(*)filter (where sentiment='neutral') as neu,
	COUNT(*)filter (where sentiment='negative') as neg,
	to_char(inserted_at, 'YYYY-MM') as dateMonth
FROM tweets_sentiments
WHERE sentiment <> ''

GROUP BY dateMonth
ORDER BY dateMonth;

"pos"	"neu"	"neg"	"datemonth"
6388	9691	7209	"2023-06"
35686	71684	44351	"2023-07"
29426	60556	32847	"2023-08"
36850	67536	46814	"2023-09"
40047	86475	61088	"2023-10"
42951	82960	55729	"2023-11"
49713	79932	61243	"2023-12"
36013	61414	48219	"2024-01"
50557	66585	42365	"2024-02"
37661	76821	57215	"2024-03"
38007	59721	40084	"2024-04"
27279	63720	41051	"2024-05"
40347	61081	58538	"2024-06"
72609	129818	77943	"2024-07"
58532	99088	71694	"2024-08"
51138	74467	50335	"2024-09"
37455	114610	63495	"2024-10"
50506	101359	65116	"2024-11"
46433	95723	57281	"2024-12"
38971	71512	52349	"2025-01"
40182	75598	51264	"2025-02"
37821	73373	43594	"2025-03"
23345	59451	33922	"2025-04"
33460	103282	63808	"2025-05"
31242	65732	45845	"2025-06"
35280	80793	45599	"2025-07"
9092	18691	14826	"2025-08"

------------------------------------------------------------------------
SELECT max(nb) FROM (Select count(*) as nb
FROM tweets
GROUP BY to_char(saved_at,'YYYY-MM-dd'))

"max"
106845

-------------------------------------------------------------------------
- COUNT BY ACCOUNT and sorted by followers


SELECT * FROM (
	SELECT 
		COUNT(*) as total,
		COUNT(*)filter (where sentiment='positive') as pos,
		COUNT(*)filter (where sentiment='neutral') as neu,
		COUNT(*)filter (where sentiment='negative') as neg,
		to_char(inserted_at, 'YYYY-MM-dd') as dateMonth,
		followers,
		screen_name
	FROM tweets_sentiments
	WHERE sentiment <> ''
	--AND to_char(inserted_at, 'YYYY-MM-dd') = '2023-11-07'

	GROUP BY dateMonth,followers,screen_name
	ORDER BY  dateMonth,followers DESC,screen_name
)
WHERE total > 10;


-----------------------------------------------------------------------
NB NEW TWEETS BY day

SELECT SUM(total),dateMonth FROM (
SELECT 
	COUNT(*) as total,
	COUNT(*)filter (where sentiment='positive') as pos,
	COUNT(*)filter (where sentiment='neutral') as neu,
	COUNT(*)filter (where sentiment='negative') as neg,
	to_char(inserted_at, 'YYYY-MM-dd') as dateMonth,
	followers,
	screen_name
FROM tweets_sentiments
WHERE sentiment <> ''
AND replies_to = ''
--AND to_char(inserted_at, 'YYYY-MM-dd') = '2023-11-07'

GROUP BY dateMonth,followers,screen_name
ORDER BY  dateMonth,followers DESC,screen_name)
GROUP BY dateMonth

-----------------------------------------------------------------------
Several counts 

SELECT SUM(total)total,SUM(pos)post,SUM(neu)neu,SUM(neg)neg,date(dateMonth) FROM (
	SELECT 
		COUNT(*) as total,
		COUNT(*)filter (where sentiment='positive') as pos,
		COUNT(*)filter (where sentiment='neutral') as neu,
		COUNT(*)filter (where sentiment='negative') as neg,
		to_char(inserted_at, 'YYYY-MM-dd') as dateMonth,
		followers,
		screen_name
	FROM tweets_sentiments
	WHERE sentiment <> ''
	-- AND text LIKE '%viol%'
	--AND replies_to = ''
	--AND quoting = 'false'
	--AND to_char(inserted_at, 'YYYY-MM-dd') = '2023-11-07'

	GROUP BY dateMonth,followers,screen_name
	ORDER BY  dateMonth,followers DESC,screen_name
	)
GROUP BY dateMonth