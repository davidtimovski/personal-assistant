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
  static formatDayMonth(date: Date, locale: string): string {
    if (date.getFullYear() === new Date().getFullYear()) {
      return date.toLocaleDateString(locale, {
        day: "numeric",
        month: "short",
      });
    }

    return date.toLocaleDateString(locale, {
      day: "numeric",
      month: "short",
      year: "2-digit",
    });
  }

  /** Format as (HH/hh):mm (AM/PM). Adjusted for time zone. */
  static formatHoursMinutes(date: Date, locale: string): string {
    return new Date(
      date.getTime() - date.getTimezoneOffset() * 60000
    ).toLocaleTimeString(locale, { hour: "2-digit", minute: "2-digit" });
  }

  /** Format as Weekday, (HH/hh):mm:ss (AM/PM). */
  static formatWeekdayTime(date: Date, locale: string): string {
    const weekday = date.toLocaleString(locale, { weekday: "long" });
    const time = date.toLocaleTimeString(locale);

    return `${weekday}, ${time}`;
  }

  /** Example: Jan, Feb, Mar. */
  static getShortMonth(date: Date, locale: string) {
    return date.toLocaleString(locale, { month: "short" });
  }

  /** Example: January, Febuary, March. */
  static getLongMonth(date: Date, locale: string) {
    return date.toLocaleString(locale, { month: "long" });
  }

  static adjustForTimeZone(date: Date): Date {
    let hoursDiff = date.getHours() - date.getTimezoneOffset() / 60;
    date.setHours(hoursDiff);

    return date;
  }
}
