/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using System.IO;

namespace maxstAR
{
    /// <summary>
    /// Serve mmap file recognition and tracking
    /// </summary>
    public abstract class AbstractSpaceTrackableBehaviour : AbstractTrackableBehaviour
	{
		/// <summary>
		/// Notify mmap file is changed. 
		/// </summary>
		/// <param name="trackerFileName">mmap file name</param>
		protected override void OnTrackerDataFileChanged(string trackerFileName)
		{
			TrackableId = null;
			if (trackerFileName == null || trackerFileName.Length == 0)
			{
				TrackableName = null;
				return;
			}

			TrackableName = Path.GetFileNameWithoutExtension(trackerFileName);
			int startIdx = trackerFileName.LastIndexOf("/") + 1;

			int endIdx = trackerFileName.LastIndexOf(".");
			if (endIdx > startIdx)
			{
				string fileName = trackerFileName.Substring(startIdx, endIdx - startIdx);
			}

		}
	}
}
