export class DateHelper {
  static format(date: Date): string {
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .split("T")[0];
  }

  static formatYYYYMM(date: Date): string {
    return date.toISOString().substring(0, 7);
  }

  static adjustForTimeZone(date: Date): Date {
    let hoursDiff = date.getHours() - date.getTimezoneOffset() / 60;
    date.setHours(hoursDiff);

    return date;
  }
}
