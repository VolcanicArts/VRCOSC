// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class DateTimeClipVariable : ClipVariable
{
    public DateTimeClipVariable()
    {
    }

    public DateTimeClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("datetime_format", "Date/Time Format", "How should the date/time be formatted?")]
    public string DateTimeFormat { get; set; } = "yyyy/MM/dd HH:mm:ss";

    [ClipVariableOption("timezone_id", "Time Zone ID", "What timezone should this date/time be converted to?\nNote: Daylight savings is handled automatically")]
    public string TimeZoneID { get; set; } = TimeZoneInfo.Local.Id;

    public override bool IsDefault() => base.IsDefault() &&
                                        DateTimeFormat == "yyyy/MM/dd HH:mm:ss" &&
                                        TimeZoneID == TimeZoneInfo.Local.Id;

    public override DateTimeClipVariable Clone()
    {
        var clone = (DateTimeClipVariable)base.Clone();

        clone.DateTimeFormat = DateTimeFormat;
        clone.TimeZoneID = TimeZoneID;

        return clone;
    }

    protected override string Format(object value)
    {
        var dateTimeValue = (DateTimeOffset)value;

        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneID);
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue.UtcDateTime, timeZoneInfo);

            try
            {
                return convertedDateTime.ToString(DateTimeFormat, CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
                return "INVALID FORMAT";
            }
        }
        catch (Exception)
        {
            return "INVALID TIMEZONE";
        }
    }
}
