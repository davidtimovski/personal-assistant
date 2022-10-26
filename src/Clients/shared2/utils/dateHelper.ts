export class DateHelper {
  /** Format as 2022-10-25. Adjusted for time zone. */
  static format(date: Date): string {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .split("T")[0];
  }

  /** Format as 2022-10-25T18:15:14.321Z. Adjusted for time zone.  */
  static formatISO(date: Date): string {
    return new Date(
      date.getTime() - date.getTimezoneOffset() * 60000
    ).toISOString();
  }

  /** Format as 25/10/2022. */
  static formatForReading(date: Date): string {
    return date.toLocaleDateString("en-GB");
  }

  /** Format as 2022-10. Adjusted for time zone. */
  static formatYYYYMM(date: Date): string {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .substring(0, 7);
  }

  /** Format to show the day, month, and possibly year in the specified locale */
  static formatDayMonth(date: Date, language: string): string {
    if (date.getFullYear() === new Date().getFullYear()) {
      return date.toLocaleDateString(language, {
        day: "numeric",
        month: "short",
      });
    }

    return date.toLocaleDateString(language, {
      day: "numeric",
      month: "short",
      year: "2-digit",
    });
  }

  /** Format as 18:15. Adjusted for time zone. */
  static formatHoursMinutes(date: Date): string {
    const timePart = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .split("T")[1];

    return timePart.substring(0, 5);
  }

  /** Format as Weekday, (HH/hh):mm:ss (AM/PM). */
  static formatWeekdayTime(date: Date, language: string): string {
    const weekday = date.toLocaleString(language, { weekday: "long" });
    const time = date.toLocaleTimeString(language);

    return `${weekday}, ${time}`;
  }

  /** Example: Jan, Feb, Mar. */
  static getShortMonth(date: Date, language: string) {
    return date.toLocaleString(language, { month: "short" });
  }

  /** Example: January, Febuary, March. */
  static getLongMonth(date: Date, language: string) {
    return date.toLocaleString(language, { month: "long" });
  }

  static adjustForTimeZone(date: Date): Date {
    let hoursDiff = date.getHours() - date.getTimezoneOffset() / 60;
    date.setHours(hoursDiff);

    return date;
  }
}
