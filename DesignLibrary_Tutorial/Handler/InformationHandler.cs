﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;


namespace AppTestProzesse.Header
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
                .Replace("&nbsp;", "")
                .Replace("|", "");
            while (source.Contains("<a href=")) //cuts out unnecessary information/ tables etc
            {
                string sndPart = source.Substring(source.IndexOf("<a href=")).Substring(source.Substring(source.IndexOf("<a href=")).IndexOf("</table>") + 8);
                source = source.Substring(0, source.IndexOf("<a href=")) + sndPart;
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
            Days day = Days.Montag;
            string[] tempChanges = new string[11];
            int regPosition = 0;

            string curr_element = "";

            bool readTableRow = false;
            List<Tuple<Days, Hours[], string, Subject>> registeredChanges = new List<Tuple<Days, Hours[], string, Subject>>(); //the day, which hours, which class and the changes to the original subject

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.AttributeCount > 0 && reader.GetAttribute("name") != null) //&& reader.Name == "a"
                        {
                            day = (Days)int.Parse(reader.GetAttribute("name")) - 1;
                        }
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
                            if(reader.Value == "---")
                            {
                                tempChanges[regPosition] = string.Empty;
                            }
                            else tempChanges[regPosition] = reader.Value;
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
                                hours.Add(stringToHour(tempChanges[2]));
                            else
                            {
                                string hString = tempChanges[2].Replace(" ", "");
                                if (hString.Contains("-"))
                                {
                                    for (int i = (int)stringToHour(hString.Substring(0, hString.IndexOf("-"))); i <= (int)stringToHour(hString.Substring(hString.IndexOf("-") + 1)); i++)
                                    {
                                        hours.Add((Hours)i);
                                    }
                                }
                                else if (hString.Contains(",")) //INFO NEEDED!
                                {
                                    for (int i = int.Parse(hString.Substring(0, hString.IndexOf(","))); i <= int.Parse(hString.Substring(hString.LastIndexOf(","))) + 1; i++)
                                    {
                                        hours.Add(stringToHour(i.ToString()));
                                    }
                                }
                            }
                            //Subject
                            SubChange change;
                            if ((tempChanges[8] != null || tempChanges[9] != null) && (!tempChanges[8].Contains("---") || !tempChanges[9].Contains("---"))) //more Information given here; there is no new room!
                                change = new SubChange(tempChanges[0], tempChanges[4], tempChanges[7], new Tuple<string, string>(tempChanges[8], tempChanges[9]), tempChanges[10]);
                            else
                                change = new SubChange(tempChanges[0], tempChanges[4], tempChanges[7], tempChanges[10]);

                            //check if type is "Entfall" -> init new bool as 4th parameter and add it to the following constroctur of subject
                            Subject subject;
                            if (change.type == "Entfall")
                                subject = new Subject(tempChanges[3], tempChanges[6], change, true);
                            else
                                subject = new Subject(tempChanges[3], tempChanges[6], change);

                            //Final Add
                            registeredChanges.Add(new Tuple<Days, Hours[], string, Subject>(day, hours.ToArray(), tempChanges[5], subject));

                            readTableRow = false;
                        }
                        break;
                }
            }
            return registeredChanges;
        }

        public void ApplyChanges(Week w, List<Tuple<Days, Hours[], string, Subject>> changes) //needs tests
        {
            if (changes.Count > 0)
            {
                foreach(var change in changes)
                {
                    if(change.Item3.Contains(w.mClass)) //same class || Changes on 05.08.17!
                    {
                        foreach(var hour in change.Item2)
                        {
                            if(w.week[(int)change.Item1].list[(int)hour] != null) //checks if lessons are available
                            {
                                foreach(var subject in w.week[(int)change.Item1].list[(int)hour])
                                {
                                    if(subject.name == change.Item4.name && (
                                        (change.Item4.room.Contains(subject.room) || change.Item4.change.newRoom.Contains(subject.room)) ||
                                        (subject.room.Contains(",") && (change.Item4.room.Contains(subject.room.Substring(0, subject.room.IndexOf(","))) || change.Item4.change.newRoom.Contains(subject.room.Substring(0, subject.room.IndexOf(","))))) )) //checks if information matches | Ex: if weeks room is already set to newRoom
                                    {
                                        subject.change = change.Item4.change; //adds new changes to original subject instance
                                        if(subject.ev != null)
                                        {
                                            if(!subject.ev.Describtion.Contains(subject.change.remarks)) //Needs to be checked
                                            {
                                                subject.ev.Describtion += " " + subject.change.remarks;
                                            }
                                        }
                                    }
                                }
                            }
                        }  
                    }
                }
            }
        }

        private Hours stringToHour(string source)
        {
            if (source == "MP" || source == "Mittagspause") //INFO NEEDED
                return Hours.MP;
            int hourIndex = int.Parse(source);
            if (hourIndex < 7)
                return (Hours)hourIndex - 1;
            return (Hours)hourIndex;
        }

        public Week ClassSourceToWeek(string source, string w_class, DateTime w_monday)
        {
            if (source == null)
                return null;

            Hours row = Hours.MP;
            string colspan = "";
            bool start = false, endTag = false;
            bool[] strike = new bool[2]; //0 = prev, 1 = current
            int colspanValue = 0, day = 0, rowspan = -1;

            string t = "";

            Queue<string> elements = new Queue<string>();
            Queue<string> endtags = new Queue<string>();
            List<Subject> lesson = new List<Subject>();
            List<string> lessonsStack = new List<string>();
            List<string> endTagTemp = new List<string>();

            Week week = new Week(w_monday, w_class);

            XDocument xmlDoc = SourceToXmlDoc_class(source);
            XmlReader reader = xmlDoc.CreateReader();

            int count = 0, eventDayCount = 0;

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
                            {
                                colspan = reader.GetAttribute("colspan");
                            }
                            if (reader.GetAttribute("rowspan") != null)
                            {
                                rowspan = int.Parse(reader.GetAttribute("rowspan"));
                            }

                            if (week.mEvents.Count > 0) //Events
                            {
                                foreach (Event ev in week.mEvents)
                                {
                                    if (ev.Day == (Days)(day % 5) && ev.Hour < row && ev.Number >= row)
                                    {
                                        day++;
                                        eventDayCount++;
                                    }
                                }
                            }

                            if (colspanValue == 12) //Lessons will be added to Day
                            {
                                AddLesson(ref lesson, ref lessonsStack, ref strike);

                                if (lesson.Count > 0)
                                {
                                    week.AddLesson((Days)(day % 5), row, lesson.ToArray());
                                    lesson.Clear();
                                }
                                day++;
                                if (colspan != "")
                                {
                                    colspanValue = int.Parse(colspan);
                                }
                                else colspanValue = 0;
                            }
                            else
                            {
                                if (colspan != "" && colspan != "2")
                                {
                                    colspanValue += int.Parse(colspan);
                                }
                            }

                            if (week.mEvents.Count > 0)
                            {
                                foreach (Event ev in week.mEvents)
                                {
                                    if (ev.Day == (Days)(day % 5) && ev.Hour < row && ev.Number >= row && lesson.Count > 0)
                                    {
                                        day++;
                                        eventDayCount++;
                                    }
                                }
                            }
                        }

                        if (reader.Name == "strike")
                        {
                            strike[0] = true;                  
                        }
                        else elements.Enqueue(reader.Name);

                        //Following code checks for Lessons with only one attribute
                        if (elements.Count > 3) elements.Dequeue();

                        if (elements.Count == 3)
                        {
                            if (elements.ToArray()[0].Contains("TR") && elements.ToArray()[1].Contains("TD") && elements.ToArray()[2].Contains("font"))
                            {
                                endTag = true;
                                endtags.Clear();
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                        if (start) AddLesson(ref lesson, ref lessonsStack, ref strike);

                        if (rowspan == 2 && colspan == "" && row != Hours.tenth) //Hinzugefügt: && row != Hours.tenth
                        {
                            if (reader.Value.Contains("MP")) row = Hours.MP;
                            else if (int.Parse(reader.Value) < 7) row = (Hours)(int.Parse(reader.Value) - 1);
                            else row = (Hours)(int.Parse(reader.Value));

                            lessonsStack.Clear();
                            start = true;
                        }
                        else if (endTag)
                        {
                            lessonsStack.Add(reader.Value);
                            if (start)
                            {
                                endTagTemp.Add(reader.Value);
                            }
                        }
                        else if (start)
                        {
                            endTagTemp.Add(reader.Value);
                        }
                        if (rowspan > 2 && rowspan % 2 == 0) //Adding Events
                        {
                            bool con = false;
                            if (week.mEvents.Count > 0) //Exception for events with a description greater than 1 lines 
                            {
                                foreach (Event ev in week.mEvents)
                                {
                                    if (ev.Day == (Days)(day % 5) && ev.Hour <= row && ev.Number >= row) //old vers.(didn't work): ev.Day == (Days)((day - eventDayCount) % 5)
                                    {
                                        con = true;
                                        break;
                                    }
                                }
                            }
                            if (con) continue;
                            t += reader.Value;
                            week.AddEvent(new Event((Days)(day % 5), row, (rowspan / 2) - 1 + row)); //CHECK!
                            /*t += eventSpace.ToArray()[eventSpace.Count - 1].Day + " "
                                + eventSpace.ToArray()[eventSpace.Count - 1].Hour + " "
                                + eventSpace.ToArray()[eventSpace.Count - 1].Number + " " + count
                                + System.Environment.NewLine;*/
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "strike")
                        {
                            strike[1] = strike[0];
                            strike[0] = false;
                        }
                        else endtags.Enqueue(reader.Name);

                        //Following code checks for Lessons with only one attribute
                        if (endtags.Count > 3) endtags.Dequeue();
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
            //week.LinkEventsToSubjects();
            //t = week.GetAll();
            return week;
        }

        private void AddLesson(ref List<Subject> lesson, ref List<string> temp, ref bool[] strike)
        {
            if (temp.Count == 2)
            {
                if (temp[0] == temp[1])
                {
                    lesson.Add(new Subject(temp[0].Replace("\n", "").Replace(".",""), "N.A.", strike[1]));
                }
                else lesson.Add(new Subject(temp[1].Replace("\n", "").Replace(".", ""), temp[0].Replace("\n", "").Replace(".", ""), strike[1]));
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
