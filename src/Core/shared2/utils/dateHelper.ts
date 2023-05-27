export class DateHelper {
  static adjustTimeZone(date: Date): Date {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  }

  /** Format as 2022-10-25. Adjusted for time zone. */
  static format(date: Date): string {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .split("T")[0];
  }

  /** Format as 25/10/2022. */
  static formatForReading(date: Date): string {
    return date.toLocaleDateString("en-GB");
  }

  /** Format as 2022-10. */
  static formatYYYYMM(date: Date): string {
    const year = date.getFullYear().toString();
    const month = date.toLocaleDateString(undefined, {
      month: "2-digit",
    });

    return `${year}-${month}`;
  }

  /** Format to show the day, month, and possibly year in the specified culture */
  static formatDayMonth(date: Date, culture: string): string {
    if (date.getFullYear() === new Date().getFullYear()) {
      return date.toLocaleDateString(culture, {
        day: "numeric",
        month: "short",
      });
    }

    return date.toLocaleDateString(culture, {
      day: "numeric",
      month: "short",
      year: "2-digit",
    });
  }

  /** Format as (HH/hh):mm (AM/PM). */
  static formatHoursMinutes(date: Date, culture: string): string {
    return date.toLocaleTimeString(culture, {
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  /** Format as (HH/hh) (AM/PM). */
  static formatHours(date: Date, culture: string): string {
    return date.toLocaleTimeString(culture, { hour: "2-digit" });
  }

  /** Format as Fri. */
  static formatWeekdayShort(date: Date, language: string): string {
    return date.toLocaleString(language, { weekday: "short" });
  }

  /** Format as Friday, (HH/hh):mm (AM/PM). */
  static formatWeekdayTime(
    date: Date,
    language: string,
    culture: string
  ): string {
    const weekday = date.toLocaleString(language, { weekday: "long" });
    const time = date.toLocaleTimeString(culture, {
      hour: "2-digit",
      minute: "2-digit",
    });

    return `${weekday}, ${time}`;
  }

  /** Format as Friday, (HH/hh):mm:ss (AM/PM). */
  static formatWeekdayHoursMinutes(
    date: Date,
    language: string,
    culture: string
  ): string {
    const weekday = date.toLocaleString(language, { weekday: "long" });
    const time = date.toLocaleTimeString(culture, {
      hour: "2-digit",
      minute: "2-digit",
    });

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
}
