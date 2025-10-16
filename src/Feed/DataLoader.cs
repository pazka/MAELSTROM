
namespace Maelstrom.Feed
{
    public struct DataPoint
    {
        public DateTime date;
        public int retweetCount;
        public float normalizedRetweetCount;
        public float normalizedDate;
    }
    
    public struct DataBound
    {
        public DataPoint Min;
        public DataPoint Max;
    }

    /// <summary>
    /// Represents data/logic for an object that controls its display object
    /// </summary>
    public class DataLoader
    {
        static private DataPoint[] _data = new DataPoint[367_479];

        static private string _dataPath = "data/feed_tweets_retweeted.csv";
        public static DataBound _dataBounds = new DataBound();

        /**
        Beginin of file is "date","retweet_count"
        "2023-02-07 00:02:07",1
        "2023-02-07 00:03:52",6
        "2023-02-07 00:13:59",20

        */
        static public void LoadData()
        {
            using var reader = new StreamReader(_dataPath);
            reader.ReadLine();
            int _dataIndex = 0;
            
            // Initialize bounds with first data point
            bool firstDataPoint = true;
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                if (line.StartsWith("date")) continue;
                var fields = line.Split(',');
                if (fields.Length < 2) continue;

                // Remove quotes from date field if present
                var dateString = fields[0].Trim('"');
                var date = DateTime.Parse(dateString);
                
                // Remove quotes from retweet count field if present
                var retweetString = fields[1].Trim('"');
                var retweetCount = int.Parse(retweetString);

                var dataPoint = new DataPoint();
                dataPoint.date = date;
                dataPoint.retweetCount = retweetCount;

                if (firstDataPoint)
                {
                    _dataBounds.Min = dataPoint;
                    _dataBounds.Max = dataPoint;
                    firstDataPoint = false;
                }
                else
                {
                    if (dataPoint.retweetCount < _dataBounds.Min.retweetCount) _dataBounds.Min.retweetCount = dataPoint.retweetCount;
                    if (dataPoint.retweetCount > _dataBounds.Max.retweetCount) _dataBounds.Max.retweetCount = dataPoint.retweetCount;
                    if (dataPoint.date < _dataBounds.Min.date) _dataBounds.Min.date = dataPoint.date;
                    if (dataPoint.date > _dataBounds.Max.date) _dataBounds.Max.date = dataPoint.date;
                }

                _data[_dataIndex] = dataPoint;
                _dataIndex++;
            }

            Console.WriteLine("Data Loaded, normalizing data...");
            Console.WriteLine($"Data bounds: {_dataBounds.Min.date:yyyy-MM-dd HH:mm:ss} to {_dataBounds.Max.date:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Retweet bounds: {_dataBounds.Min.retweetCount} to {_dataBounds.Max.retweetCount}");
            Console.WriteLine($"Logarithmic retweet bounds: {Math.Log(_dataBounds.Min.retweetCount + 1):F3} to {Math.Log(_dataBounds.Max.retweetCount + 1):F3}");
            
            for (int i = 0; i < _dataIndex; i++)
            {
                // Logarithmic normalization for retweet count
                // Add 1 to avoid log(0) and ensure all values are positive
                float logMin = (float)Math.Log(_dataBounds.Min.retweetCount + 1);
                float logMax = (float)Math.Log(_dataBounds.Max.retweetCount + 1);
                float logCurrent = (float)Math.Log(_data[i].retweetCount + 1);
                
                _data[i].normalizedRetweetCount = (logCurrent - logMin) / (logMax - logMin);
                _data[i].normalizedDate = (float)((_data[i].date - _dataBounds.Min.date).TotalSeconds / (_dataBounds.Max.date - _dataBounds.Min.date).TotalSeconds);
            }
            Console.WriteLine("Data normalized");
        }

        /// <summary>
        /// Get normalized duration for a given time span
        /// </summary>
        public static float GetNormalizedDuration(TimeSpan duration)
        {
            return (float)(duration.TotalSeconds / (_dataBounds.Max.date - _dataBounds.Min.date).TotalSeconds);
        }

        /// <summary>
        /// Get all data points
        /// </summary>
        public static DataPoint[] GetData()
        {
            return _data;
        }

        /// <summary>
        /// Get data bounds
        /// </summary>
        public static DataBound GetDataBounds()
        {
            return _dataBounds;
        }
    }
}
