export class ValidationUtil {
  static isEmptyOrWhitespace(text: string | null): boolean {
    return !text || !text.replace(/\s/g, "").length;
  }

  /** Inclusive between. */
  static between(value: number | null, from: number, to: number): boolean {
    if (!value) {
      return false;
    }

    return value >= from && value <= to;
  }

  static sameOrHigher(value: number | null, from: number): boolean {
    if (!value) {
      return false;
    }

    return value >= from;
  }
}

export class ValidationResult {
  erroredFields: string[] = [];

  constructor(public valid = true) {}

  fail(field: string) {
    this.valid = false;
    this.erroredFields.push(field);
  }
}
