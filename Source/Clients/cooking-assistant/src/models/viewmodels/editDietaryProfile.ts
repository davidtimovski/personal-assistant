import { DailyIntake } from "./dailyIntake";

export class EditDietaryProfile {
  constructor(
    public birthday: string,
    public gender: string,
    public heightCm: number,
    public heightFeet: number,
    public heightInches: number,
    public weightKg: number,
    public weightLbs: number,
    public activityLevel: string,
    public goal: string,
    public dailyIntake: DailyIntake
  ) {}
}
