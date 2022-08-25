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
}

export class ValidationResult {
  erroredFields: string[] = [];

  constructor(public valid: boolean) {}

  fail(field: string) {
    this.valid = false;
    this.erroredFields.push(field);
  }
}
