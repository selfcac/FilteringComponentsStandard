using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TimeBlockFilter;

namespace TimeBlockFilter_Tests
{
    [TestClass()]
    public class TimeFilterObjectTests
    {


        static DateTime dateString(string date)
        {
            return DateTime.ParseExact(
                date,
                "d/M/yyyy h:mm:ss tt",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        [TestMethod()]
        public void isBlockedTest()
        {
            TimeFilterObject filter = new TimeFilterObject();
            filter.clearAllTo(true);
            filter.setPolicy(DayOfWeek.Monday, 6, false);

            // 1 April 2019 = Monday

            Assert.IsTrue(filter.isBlocked(new DateTime(2019, 4, 1, 6, 15, 00)));
            Assert.IsFalse(filter.isBlocked(new DateTime(2019, 4, 2, 6, 15, 00)));

            Assert.IsFalse(filter.isBlocked(new DateTime(2019, 4, 1, 5, 59, 59)));
            Assert.IsFalse(filter.isBlocked(new DateTime(2019, 4, 1, 7, 0, 00)));

            filter.setPolicy(DayOfWeek.Tuesday, 0, false);

            Assert.IsTrue(filter.isBlocked(dateString("2/4/2019 12:20:00 AM")));
            Assert.IsFalse(filter.isBlocked(dateString("2/4/2019 1:00:00 AM")));
            Assert.IsFalse(filter.isBlocked(dateString("2/4/2019 12:20:00 PM")));
            Assert.IsFalse(filter.isBlocked(dateString("1/4/2019 11:59:59 PM")));
        }

        [TestMethod()]
        public void SingleArray()
        {
            TimeFilterObject filter = new TimeFilterObject();
            filter.clearAllTo(true);

            // Test for one array (error used '*')
            filter.setPolicy((DayOfWeek)0, 1, false);
            filter.setPolicy((DayOfWeek)0, 0, true);
            Assert.IsTrue(filter.isBlocked(new DateTime(2019, 4, 28, 1, 00, 00)));
        }

        public DateTime CreateDayOfWeek(int DayOfWeek, int hour, int min)
        {
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysUntilTuesday = (DayOfWeek - (int)dt.DayOfWeek + 7) % 7;
            //  DateTime nextTuesday = today.AddDays(daysUntilTuesday);
            dt = dt.AddDays(daysUntilTuesday);
            return dt;
        }

        [TestMethod()]
        public void SingleArrayExtender()
        {
            TimeFilterObject filter = new TimeFilterObject();

            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    filter.clearAllTo(true);

                    // Test for one array (error used '*')
                    filter.setPolicy((DayOfWeek)day, hour, false);
                    Assert.IsTrue(
                        filter.isBlocked(CreateDayOfWeek(day, hour, 0)),
                        string.Format("day {0} hour {1}", day, hour)
                        );
                }
            }
        }

        [TestMethod()]
        public void SingleArrayOrder()
        {
            // Make sure the structure is [..24h of day 1 .. 24h of day 2...] = [24hours per day  * 7 ]
            // and not [ 0-1AM across days , 1-2AM across days ...] = [ hour per week * 24  ]

            TimeFilterObject filter = new TimeFilterObject();

            filter.clearAllTo(false);

            filter.setPolicy(0, 0, true);
            filter.setPolicy((DayOfWeek)1, 5, true);
            filter.setPolicy((DayOfWeek)2, 0, true);
            filter.setPolicy((DayOfWeek)4, 1, true);

            // Correct should be : 24*day + hour
            Assert.AreEqual(true, filter.AllowDayAndTimeMatrix[24 * 0 + 0]);
            Assert.AreEqual(true, filter.AllowDayAndTimeMatrix[24 * 1 + 5]);
            Assert.AreEqual(true, filter.AllowDayAndTimeMatrix[24 * 2 + 0]);
            Assert.AreEqual(true, filter.AllowDayAndTimeMatrix[24 * 4 + 1]);

            //Wrong chek that it is not : 24*hour + day
            Assert.AreEqual(false, filter.AllowDayAndTimeMatrix[24 * 0 + 1]);

        }

    }
}
