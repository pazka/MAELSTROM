select COUNT(*) from twitter_data 
WHERE sentiment IS NOT NULL;

SELECT * FROM twitter_data 
WHERE sentiment <> ''
ORDER BY inserted_at ASC
LIMIT 100;


SELECT COUNT(*),MAX(inserted_at),MIN(inserted_at) FROM twitter_data 
WHERE sentiment <> '';

SELECT 
	COUNT(*)filter (where sentiment='positive') as pos,
	COUNT(*)filter (where sentiment='neutral') as neu,
	COUNT(*)filter (where sentiment='negative') as neg,
	to_char(inserted_at, 'YYYY-MM') as dateMonth
FROM twitter_data
WHERE sentiment <> ''

GROUP BY dateMonth
ORDER BY dateMonth;
