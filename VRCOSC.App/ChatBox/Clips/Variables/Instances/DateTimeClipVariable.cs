// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class DateTimeClipVariable : ClipVariable
{
    public DateTimeClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("Date/Time Format", "datetime_format")]
    public string DateTimeFormat { get; set; } = string.Empty;

    [ClipVariableOption("Time Zone ID", "timezone_id")]
    public string TimeZoneID { get; set; } = TimeZoneInfo.Local.Id;

    protected override string Format(object value)
    {
        var dateTimeValue = (DateTimeOffset)value;

        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneID);
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue.UtcDateTime, timeZoneInfo);

            if (string.IsNullOrEmpty(DateTimeFormat))
            {
                return convertedDateTime.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                return convertedDateTime.ToString(DateTimeFormat, CultureInfo.CurrentCulture);
            }
        }
        catch (Exception)
        {
            return "INVALID TIMEZONE";
        }
    }
}
