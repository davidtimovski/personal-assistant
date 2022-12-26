export class Tooltip {
  title: string;
  question: string;
  answer: string;

  constructor(public key: string, public isDismissed: boolean) {}
}
