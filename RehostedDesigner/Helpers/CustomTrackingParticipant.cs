﻿using System;
using System.Activities.Tracking;

namespace RehostedWorkflowDesigner.Helpers
{

    /// <summary>
    /// Workflow Tracking Participant - Custom Implementation
    /// </summary>
    internal class CustomTrackingParticipant : TrackingParticipant
    {
        public string TrackData = string.Empty;

        /// <summary>
        /// Appends the current TrackingRecord data to the Workflow Execution Log
        /// </summary>
        /// <param name="trackRecord">Tracking Record Data</param>
        /// <param name="timeStamp">Timestamp</param>
        protected override void Track(TrackingRecord trackRecord, TimeSpan timeStamp)
        {
            ActivityStateRecord recordEntry = trackRecord as ActivityStateRecord;

            if (recordEntry != null)
            {
                    TrackData += string.Format("[{0}] [{1}] [{2}]" + Environment.NewLine, recordEntry.EventTime.ToLocalTime(), recordEntry.Activity.Name, recordEntry.State);
            }
        }
    }
}
