export class ValidationUtil {
  static isEmptyOrWhitespace(text: string): boolean {
    return !text || !text.replace(/\s/g, "").length;
  }
}
