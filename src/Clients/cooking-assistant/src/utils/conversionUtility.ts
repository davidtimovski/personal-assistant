import { inject } from "aurelia-framework";
import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";

// MAYBE SOME DAY IN THE FUTURE I WILL USE THIS
@inject(LocalStorageCurrencies)
export class ConvertionUtility {
  private readonly gramsInOunce = 28.3495;
  private readonly gramsInCup = 220;
  private readonly gramsInTablespoon = 14.15;
  private readonly gramsInTeaspoon = 4.2;
  private readonly poundsInKilo = 2.2046;
  private readonly centimetersInFoot = 30.48;
  private readonly centimetersInInch = 2.54;

  constructor(private readonly localStorage: LocalStorageCurrencies) {}

  convertToGrams(unit: string, amount: number): number {
    switch (unit) {
      case "g":
      case "ml":
        return amount;
      case "oz":
        return this.gramsInOunce * amount;
      case "cup":
        return this.gramsInCup * amount;
      case "tbsp":
        return this.gramsInTablespoon * amount;
      case "tsp":
        return this.gramsInTeaspoon * amount;
      default:
        throw "Invalid unit";
    }
  }

  convertToMilligrams(unit: string, amount: number): number {
    switch (unit) {
      case "g":
      case "ml":
        return amount * 1000;
      case "oz":
        return this.gramsInOunce * amount * 1000;
      case "cup":
        return this.gramsInCup * amount * 1000;
      case "tbsp":
        return this.gramsInTablespoon * amount * 1000;
      case "tsp":
        return this.gramsInTeaspoon * amount * 1000;
      default:
        throw "Invalid unit";
    }
  }

  convertFeetAndInchesToCentimeters(feet: number, inches: number): number {
    const feetInCm = feet * this.centimetersInFoot;
    const inchesInCm = inches * this.centimetersInInch;
    const cm = feetInCm + inchesInCm;
    return cm;
  }

  convertCentimetersToFeetAndInches(centimeters: number): Array<number> {
    const totalInches = Math.floor(centimeters / this.centimetersInInch);
    const feet = (totalInches - (totalInches % 12)) / 12;
    const inches = totalInches % 12;
    return [feet, inches];
  }

  convertPoundsToKilos(pounds: number): number {
    return pounds / this.poundsInKilo;
  }

  convertKilosToPounds(kilos: number): number {
    const pounds = kilos * this.poundsInKilo;
    return Math.round(pounds);
  }
}
