﻿using ClassroomAssignment.Model.Utils;
using ClassroomAssignment.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ClassroomAssignment.Model.DataConstants;

namespace ClassroomAssignment.Model
{
	/// <summary>
	/// Query expressions for the courses.
	/// </summary>
    public static class CourseQueryRules
    {
		/// <summary>
		/// Determines if a course needs a room.
		/// </summary>
		/// <param name="course">A course object.</param>
		/// <returns>needsRoom is set to false if the passed course doesn't not need a room assignment. True otherwise.</returns>
        public static bool QueryNeedsRoom(this Course course)
        {
            bool needsRoom = true;
            if (course.InstructionMethod?.Contains(InstructionMethods.OFF_CAMPUS) == true)
            {
                needsRoom = false;
            }
            else if (course.Room?.Contains(RoomOptions.NO_ROOM_NEEDED) == true)
            {
                needsRoom = false;
            }
            else if (course.MeetingPattern?.Contains(MeetingPatternOptions.DOES_NOT_MEET) == true)
            {
                needsRoom = false;
            }

            return needsRoom;
        }

		/// <summary>
		/// Determines if a course has an ambigous assignment.
		/// </summary>
		/// <param name="course">A course object.</param>
		/// <returns>HasMultipleRoomAssignments(course)</returns>
        public static bool QueryHasAmbiguousAssignment(this Course course)
        {
            return HasMultipleRoomAssignments(course);
        }

		/// <summary>
		/// Check if a course has multiple assignments.
		/// </summary>
		/// <param name="course">A course object.</param>
		/// <returns>True if course has multiple assignments. False otherwise.</returns>
        private static bool HasMultipleRoomAssignments(this Course course)
        {
            bool multipleAssignments = false;

            Regex longPKI = new Regex(RoomOptions.PETER_KIEWIT_INSTITUTE_REGEX);
            Regex shortPKI = new Regex(RoomOptions.PKI_REGEX);

            Match roomColMatch = longPKI.Match(course.Room);
            Match commentColMatch = shortPKI.Match(course.Comments);
            Match notesColMatch = shortPKI.Match(course.Notes);

            if (roomColMatch.Success || commentColMatch.Success || notesColMatch.Success)
            {
                if (roomColMatch.Success ^ commentColMatch.Success ^ notesColMatch.Success)
                {
                    multipleAssignments = false; ;
                }
                else
                {
                    multipleAssignments = true;
                }
            }

            return multipleAssignments;
        }

		/// <summary>
		/// Determines the meeting days of a course.
		/// </summary>
		/// <param name="course">A course object.</param>
		/// <returns>List of meeting days.</returns>
        public static List<DayOfWeek> QueryMeetingDays(this Course course)
        {
            Regex regex = new Regex(DataConstants.MeetingPatternOptions.TIME_PATTERN);
            Match match = regex.Match(course.MeetingPattern);

            List<DayOfWeek> MeetingDays = new List<DayOfWeek>();
            foreach (Capture capture in match.Groups[1].Captures)
            {
                DayOfWeek day = DateUtil.AbbreviationToDayOfWeek(capture.Value);
                MeetingDays.Add(day);
            }

            return MeetingDays;
        }

		/// <summary>
		/// Determine the start time of a course.
		/// </summary>
		/// <param name="course">A course object.</param>
		/// <returns>dateTime.TimeOfDay if the regex match is successful. Null otherwise.</returns>
		public static TimeSpan? QueryStartTime(this Course course)
        {
            Regex regex = new Regex(MeetingPatternOptions.TIME_PATTERN);
            Match match = regex.Match(course.MeetingPattern);

            if (match.Success)
            {
                var startTimeStr = match.Groups[2].Value;

                DateTime dateTime = new DateTime();
                if (DateTime.TryParse(startTimeStr, out dateTime))
                {
                    return dateTime.TimeOfDay;
                }
            }

            return null;            
        }

		/// <summary>
		/// Determine the end time of a course.
		/// </summary>
		/// <param name="course">A course object.</param>
		/// <returns>dateTime.TimeOfDay if the regex match is successful. Null otherwise.</returns>
		public static TimeSpan? QueryEndTime(this Course course)
        {
            Regex regex = new Regex(MeetingPatternOptions.TIME_PATTERN);
            Match match = regex.Match(course.MeetingPattern);

            if (match.Success)
            {
                var endTimeStr = match.Groups[3].Value;

                DateTime dateTime = new DateTime();
                if (DateTime.TryParse(endTimeStr, out dateTime))
                {
                    return dateTime.TimeOfDay;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns rooms assignments.
        /// </summary>
        /// <param name="course">A course object.</param>
        /// <returns>List of rooms assignments. Multiple rooms if course has ambiguous assignment or
        ///  an empty list if course has none.</returns>
        public static List<string> QueryRoomAssignment(this Course course)
        {
            List<string> roomAssignments = new List<string>();

            Regex[] regexs = new Regex[] { new Regex(RoomOptions.PETER_KIEWIT_INSTITUTE_REGEX), new Regex(RoomOptions.PKI_REGEX) };
            string[] courseProperties = new string[] { course.Room, course.Comments, course.Notes };
           
            foreach (var property in courseProperties)
            {
                foreach (var regex in regexs)
                {
                    Match match;
                    if ((match = regex.Match(property)).Success)
                    {
                        var normalizedRoomName = match.Value.Replace("Peter Kiewit Institute", "PKI");
                        roomAssignments.Add(normalizedRoomName);
                    }
                }
            }

            return roomAssignments;
        }


    }
}
