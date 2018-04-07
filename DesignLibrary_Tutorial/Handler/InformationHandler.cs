using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ScheduleApp.Objects;


namespace ScheduleApp.Handler
{
    public class InformationHandler
    {
        private XDocument SourceToXmlDoc_class(string source)
        {
            source = source
                .Substring(0, source.IndexOf("</body"))
                .Substring(source.IndexOf(ToStartTag("CENTER")))
                .Replace("<BR>", "")
                .Replace("&nbsp;", "")
                .Replace("nowrap=1", "nowrap=\"1\"")
                .Replace("<TABLE cellspacing=\"1\" cellpadding=\"1\"><TR><TD valign=bottom> <font  size=\"4\" face=\"Arial\" color=\"#0000FF\"></TR></TABLE>", "");
            for (int i = 22; i >= 1; i--)
            {
                source = source
                    .Replace("colspan=" + i, "colspan=\"" + i + "\"")
                    .Replace("rowspan=" + i, "rowspan=\"" + i + "\"");
            }
            return XDocument.Parse(source);
        }

        private XDocument SourceToXmlDoc_all(string source)
        {
            source = source
                .Substring(0, source.IndexOf("</body"))
                .Substring(source.IndexOf(ToStartTag("CENTER")))
                .Replace("<BR>", "")
                .Replace("<p>", "")
                .Replace("<br>", "")
                .Replace("&nbsp;", "---") //Important to replace it NOT WITH a blank space
                .Replace("|", "");
            while (source.Contains("<a href=")) //cuts out unnecessary information/ tables etc
            {
                string sndPart  = source.Substring(source.IndexOf("<a href=")).Substring(source.Substring(source.IndexOf("<a href=")).IndexOf("</table>") + 8);
                source          = source.Substring(0, source.IndexOf("<a href=")) + sndPart;
            }
            return XDocument.Parse(source);
        }

        public List<Tuple<Days, Hours[], string, Subject>> GetDetailedInfo(string source)
        {
            if (source == null) //temporary solution 
                return null;

            //Initializes reader and
            XmlReader reader = SourceToXmlDoc_all(source).CreateReader();

            //other necessary variables
            Days day                = Days.Montag;
            string[] tempChanges    = new string[11];
            int regPosition         = 0;

            string curr_element     = string.Empty;

            bool readTableRow       = false;
            List<Tuple<Days, Hours[], string, Subject>> registeredChanges = new List<Tuple<Days, Hours[], string, Subject>>(); //the day, which hours, which class and the changes to the original subject

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.AttributeCount > 0 && reader.GetAttribute("name") != null) //&& reader.Name == "a"
                            day = (Days)int.Parse(reader.GetAttribute("name")) - 1;

                        else if (reader.AttributeCount > 0 && reader.GetAttribute("class") != null && (reader.GetAttribute("class") == "list even" || reader.GetAttribute("class") == "list odd"))
                        {
                            curr_element = reader.Name;
                            readTableRow = true;
                            regPosition = 0;
                            tempChanges.Initialize();
                        }
                        break;

                    case XmlNodeType.Text:
                        if (readTableRow && reader.Value != "")
                        {
                            if (reader.Value == "---" || reader.Value == "???")
                                tempChanges[regPosition] = string.Empty;
                            else
                                tempChanges[regPosition] = reader.Value;
                            regPosition++;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (curr_element == reader.Name && readTableRow && tempChanges[0] != null)
                        {
                            //variables
                            List<Hours> hours = new List<Hours>();

                            //Hour
                            if (tempChanges[2].Length < 3)
                                hours.Add(StringToHour(tempChanges[2]));
                            else
                            {
                                string hString = tempChanges[2].Replace(" ", "");
                                if (hString.Contains("-"))
                                {
                                    for (int i = (int)StringToHour(hString.Substring(0, hString.IndexOf("-"))); i <= (int)StringToHour(hString.Substring(hString.IndexOf("-") + 1)); i++)
                                    {
                                        hours.Add((Hours)i);
                                    }
                                }
                                else if (hString.Contains(",")) //INFO NEEDED!
                                {
                                    for (int i = int.Parse(hString.Substring(0, hString.IndexOf(","))); i <= int.Parse(hString.Substring(hString.LastIndexOf(","))) + 1; i++)
                                    {
                                        hours.Add(StringToHour(i.ToString()));
                                    }
                                }
                            }
                            //Subject
                            SubChange change;
                            string room = "";
                            if (tempChanges[7] != null && tempChanges[7].Length > 0 && !tempChanges[7].Contains("---") && (tempChanges[6] == null || !tempChanges[6].Contains(tempChanges[7])))
                                room = tempChanges[7];
                            //Checks for unuseful data like dates
                            if ((tempChanges[8].Contains("-") && tempChanges[8].Contains(".") && tempChanges[8].Contains("/")) || tempChanges[8] == tempChanges[0])
                                tempChanges[8] =  null;

                            if ((tempChanges[9].Contains("-") && tempChanges[9].Contains(".") && tempChanges[9].Contains("/")) || tempChanges[9] == tempChanges[0])
                                tempChanges[9] = null;

                            if (tempChanges[8] != null || tempChanges[9] != null) //more Information given here; there is no new room!
                                change = new SubChange(tempChanges[0], tempChanges[4], room, new Tuple<string, string>(tempChanges[8], tempChanges[9]), tempChanges[10]);
                            else
                                change = new SubChange(tempChanges[0], tempChanges[4], room, tempChanges[10]);

                            //check if type is "Entfall" -> init new bool as 4th parameter and add it to the following constroctur of subject
                            Subject subject;
                            if (change.mType == "Entfall")
                                subject = new Subject(tempChanges[3], tempChanges[6], change, true);
                            else
                                subject = new Subject(tempChanges[3], tempChanges[6], change);

                            //Checks class string for "-"
                            if (tempChanges[5].Contains("-"))
                                tempChanges[5] = GetIncludedClasses(tempChanges[5]);

                            //Final assembly
                            registeredChanges.Add(new Tuple<Days, Hours[], string, Subject>(day, hours.ToArray(), tempChanges[5], subject));
                            Array.Clear(tempChanges, 0, tempChanges.Length);
                            readTableRow = false;
                        }
                        break;
                }
            }
            return registeredChanges;
        }

        private string GetIncludedClasses(string cls)
        {
            string singleCl = "";
            if (cls.Contains(";"))
            {
                if (cls.IndexOf(";") < cls.IndexOf("-"))
                {
                    singleCl    = cls.Substring(0, cls.IndexOf(";"));
                    cls         = cls.Substring(cls.IndexOf(";") + 1);
                }
                else
                {
                    singleCl    = cls.Substring(cls.IndexOf(";") + 1);
                    cls         = cls.Substring(0, cls.IndexOf(";"));
                }
            }
            int.TryParse(cls.Substring(0, cls.IndexOf('-')), out int start);
            int.TryParse(cls.Substring(cls.IndexOf('-') + 1), out int end);

            cls = "";
            for (int i = start; i <= end; i++)
            {
                cls += i.ToString() + ",";
            }
            if (cls == "")
                return singleCl;
            return cls + singleCl;
        }

        public void ApplyChanges(Week w, List<Tuple<Days, Hours[], string, Subject>> changes) //needs tests
        {
            if (changes == null || changes.Count == 0 || w == null)
                return;

            foreach (var change in changes)
            {
                if (change.Item3 == null || !change.Item3.Contains(w.mClass))
                    continue;

                foreach (var hour in change.Item2)
                {
                    if (w.mWeek[(int)change.Item1].mSubs[(int)hour] == null)
                        continue; //checks if lessons are available

                    foreach (var subject in w.mWeek[(int)change.Item1].mSubs[(int)hour])
                    {
                        if (subject.mName == change.Item4.mName && (
                            (change.Item4.mRoom.Contains(subject.mRoom) || change.Item4.mChange.mNewRoom.Contains(subject.mRoom)) ||
                            (subject.mRoom.Contains(",") && (change.Item4.mRoom.Contains(subject.mRoom.Substring(0, subject.mRoom.IndexOf(","))) || change.Item4.mChange.mNewRoom.Contains(subject.mRoom.Substring(0, subject.mRoom.IndexOf(","))))))) //checks if information matches | Ex: if weeks room is already set to newRoom
                        {
                            subject.mChange = change.Item4.mChange; //adds new changes to original subject instance
                            if (subject.mChange != null && change.Item4.mRoom != null && change.Item4.mRoom.Length > 0)
                                subject.mRoom = change.Item4.mRoom;

                            if (subject.mEvent == null)
                                continue;

                            if (!subject.mEvent.mDescribtion.Contains(subject.mChange.mRemarks)) //Needs to be checked
                                subject.mEvent.mDescribtion += " " + subject.mChange.mRemarks;
                        }
                    }
                }
            }
        }

        private Hours StringToHour(string source)
        {
            if (source == "MP" || source == "Mittagspause") //INFO NEEDED
                return Hours.MP;

            int hourIndex = int.Parse(source);
            if (hourIndex < 7)
                return (Hours)hourIndex - 1;
            return (Hours)hourIndex;
        }

        private void GoToNextDay(ref int day, ref Hours hour)
        {
            day++;
            if (day % 5 == 0) //If the next day is Monday, the Hour must be increased by one
                hour++;
        }

        public Week ClassSourceToWeek(string source, string w_class, DateTime w_monday)
        {
            if (source == null)
                return null;
            //to log

            Hours row       = Hours.MP;
            string colspan  = "";
            bool startTag   = false;
            bool endTag     = false;
            bool[] strike   = new bool[2]; //0 = prev, 1 = current
            int colspanValue = 0, day = 0, rowspan = -1;
            int count       = 0, eventDayCount = 0;

            //string t = "";

            Queue<string> elements  = new Queue<string>();
            Queue<string> endtags   = new Queue<string>();
            List<Subject> lesson    = new List<Subject>();
            List<string> lessonsStack = new List<string>();
            List<string> endTagTemp = new List<string>();

            Week week = new Week(w_monday, w_class);

            XDocument xmlDoc = SourceToXmlDoc_class(source);
            XmlReader reader = xmlDoc.CreateReader();

            while (reader.Read())
            {
                count++;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        endTag = false;
                        if (reader.AttributeCount > 0 && (reader.GetAttribute("colspan") != null || reader.GetAttribute("rowspan") != null))
                        {
                            colspan = "";
                            rowspan = -1;

                            if (reader.GetAttribute("colspan") != null && reader.GetAttribute("colspan") != "2") //"2" added!
                                colspan = reader.GetAttribute("colspan");

                            if (reader.GetAttribute("rowspan") != null)
                                rowspan = int.Parse(reader.GetAttribute("rowspan"));

                            if (week.mEvents.Count > 0) //Events
                            {
                                bool nextDay = true;
                                for (int i = 0; i < 5 && nextDay; i++)
                                {
                                    nextDay = false;
                                    foreach (Event ev in week.mEvents)
                                    {
                                        if (ev.mDay == (Days)(day % 5) && ev.mHour < row && ev.mNumber >= row)
                                        {
                                            //t += count + "Day " + day + "/ New Day " + day + 1 + " / Event " + ev.mDescribtion + " / Hour " + (Hours)row +"  (1)\n";
                                            GoToNextDay(ref day, ref row);
                                            eventDayCount++;
                                            nextDay = true;
                                        }
                                    }
                                }
                            }

                            if (colspanValue == 12) //Lessons will be added to Day
                            {
                                AddLesson(ref lesson, ref lessonsStack, ref strike);

                                if (lesson.Count > 0)
                                {
                                    //t += " --- ADD --- Day" + (Days)(day % 5) + " / ROW:" + row + " / Sub: " + lesson[0].mName + "\n";
                                    week.AddLesson((Days)(day % 5), row, lesson.ToArray());
                                    lesson.Clear();
                                }
                                //t += count + "Day " + day + "/ New Day " + day + 1 + reader.Value + " / Hour " + (Hours)row + "   (2)\n";
                                GoToNextDay(ref day, ref row);
                                if (colspan != "")
                                    colspanValue = int.Parse(colspan);
                                else
                                    colspanValue = 0;
                            }
                            else if (colspan != "" && colspan != "2")
                                colspanValue += int.Parse(colspan);

                            if (week.mEvents.Count > 0)
                            {
                                bool nextDay = true;
                                for (int i = 0; i < 5 && nextDay; i++)
                                {
                                    nextDay = false;
                                    foreach (Event ev in week.mEvents)
                                    {
                                        if (ev.mDay == (Days)(day % 5) && ev.mHour < row && ev.mNumber >= row)
                                        {
                                            //t += count + "Day " + day + "/ New Day " + day + 1 + " / Event " + ev.mDescribtion + " / Hour " + (Hours)row + "  (3)\n";
                                            GoToNextDay(ref day, ref row);
                                            eventDayCount++;
                                            nextDay = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (reader.Name == "strike")
                            strike[0] = true;
                        else
                            elements.Enqueue(reader.Name);

                        //Following code checks for Lessons with only one attribute
                        if (elements.Count > 3)
                            elements.Dequeue();

                        if (elements.Count == 3 && elements.ToArray()[0].Contains("TR") && elements.ToArray()[1].Contains("TD") && elements.ToArray()[2].Contains("font"))
                        {
                            endTag = true;
                            endtags.Clear();
                        }
                        break;

                    case XmlNodeType.Text:
                        if (startTag)
                            AddLesson(ref lesson, ref lessonsStack, ref strike);

                        if (rowspan == 2 && colspan == "" && row != Hours.tenth) //Hinzugefügt: && row != Hours.tenth
                        {
                            if (reader.Value.Contains("MP"))
                                row = Hours.MP;
                            else if (!Regex.IsMatch(reader.Value, @"[a-zA-Z]"))
                            {
                                if (int.Parse(reader.Value) < 7)
                                    row = (Hours)(int.Parse(reader.Value) - 1);
                                else
                                    row = (Hours)(int.Parse(reader.Value));
                            }
                            lessonsStack.Clear();
                            startTag = true;
                        }
                        else if (endTag)
                        {
                            lessonsStack.Add(reader.Value);
                            if (startTag)
                                endTagTemp.Add(reader.Value);
                        }
                        else if (startTag)
                            endTagTemp.Add(reader.Value);

                        if (rowspan > 2 && rowspan % 2 == 0) //Adding Events
                        {
                            bool con = false;
                            if (week.mEvents.Count > 0) //Exception for events with a description greater than 1 lines 
                            {
                                foreach (Event ev in week.mEvents)
                                {
                                    if (ev.mDay == (Days)(day % 5) && ev.mHour <= row && ev.mNumber >= row) //old vers.(didn't work): ev.Day == (Days)((day - eventDayCount) % 5)
                                    {
                                        con = true;
                                        break;
                                    }
                                }
                            }
                            if (con && eventDayCount == 0)
                                continue;

                            //t += count + " Day " + (Days)(day % 5) + "/ Von " + row + " / Ende " + ((rowspan / 2) - 1 + row) + "\n";
                            week.AddEvent(new Event((Days)(day % 5), row, (rowspan / 2) - 1 + row)); //CHECK!
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "strike")
                        {
                            //!(lessonsStack.Count == 2 && strike[1]) ||
                            if ( lessonsStack.Count == 0 || ( lessonsStack.Count == 2 && lessonsStack[0].Contains("\n") ) ) //It must be only one object inside -> else there appeared blank spaces with "strike" -> incorrect
                                strike[0] = false;
                            strike[1] = strike[0];
                            strike[0] = false;
                        }
                        else endtags.Enqueue(reader.Name);

                        //Following code checks for Lessons with only one attribute
                        if (endtags.Count > 3)
                            endtags.Dequeue();
                        if (endtags.Count == 3 && endTagTemp.Count > 0)
                        {
                            if (endtags.ToArray()[0].Contains("font") && endtags.ToArray()[1].Contains("TD") && endtags.ToArray()[2].Contains("TR"))
                            {
                                lessonsStack.Add(endTagTemp[endTagTemp.Count - 1]);
                                endTagTemp.Clear();
                            }
                        }
                        else endTag = false;
                        break;
                }
            }

            //Check
            for (int i = 0; i < 5; i++) 
            {
                if (week.mWeek[i].mSubs[6] == null || !week.mWeek[i].mSubs[6][0].mRoom.Contains("N.A."))
                    continue;

                if (week.mWeek[i].mSubs[5] != null && week.mWeek[i].mSubs[5][0].mEvent != null)
                {
                    week.mWeek[i].mSubs[5][0].mEvent.mNumber++;
                    week.mWeek[i].mSubs[6][0].mEvent = week.mWeek[i].mSubs[5][0].mEvent;
                } 

                else if (week.mWeek[i].mSubs[7] != null && week.mWeek[i].mSubs[7][0].mEvent != null)
                {
                    week.mWeek[i].mSubs[7][0].mEvent.mHour--;
                    week.mWeek[i].mSubs[6][0].mEvent = week.mWeek[i].mSubs[7][0].mEvent;
                }
            }

            //week.LinkEventsToSubjects();
            //t = week.GetAll();
            return week;
        }

        private void AddLesson(ref List<Subject> lesson, ref List<string> temp, ref bool[] strike)
        {
            if (temp.Count == 2)
            {
                if (temp[0] == temp[1])
                    lesson.Add(new Subject(temp[0].Replace(".\n", "").Replace("\n", ""), "N.A.", strike[1]));
                else
                    lesson.Add(new Subject(temp[1].Replace(".\n", "").Replace("\n", ""), temp[0].Replace(".\n", "").Replace("\n", ""), strike[1]));
                strike[1] = strike[0];
                strike[0] = false;
                temp.Clear();
            }
        }

        private static string ToStartTag(string s)
        {
            return "<" + s + ">";
        }

        private static string ToEndTag(string s)
        {
            return "</" + s + ">";
        }
    }
}
