export class ValidationUtil {
  constructor() {}

  static isEmptyOrWhitespace(text: string): boolean {
    return !text || !text.replace(/\s/g, "").length;
  }
}
