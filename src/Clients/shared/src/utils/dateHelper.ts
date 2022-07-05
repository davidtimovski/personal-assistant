export class DateHelper {
  /** Format as yyyy-MM-dd. */
  static format(date: Date): string {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().split("T")[0];
  }

  /** Format as dd/MM/yyyy. */
  static formatForReading(date: Date): string {
    return date.toLocaleDateString("en-GB");
  }

  /** Format as yyyy-MM. */
  static formatYYYYMM(date: Date): string {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().substring(0, 7);
  }

  /** Format as HH:mm. */
  static formatHoursMinutes(date: Date): string {
    const timePart = new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().split("T")[1];

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
