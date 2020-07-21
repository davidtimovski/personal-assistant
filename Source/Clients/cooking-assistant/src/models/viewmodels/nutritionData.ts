export class NutritionData {
  constructor(
    public isSet: boolean,
    public servingSize: number,
    public servingSizeIsOneUnit: boolean,
    public calories: number,
    public fat: number,
    public saturatedFat: number,
    public transFat: number,
    public carbohydrate: number,
    public sugars: number,
    public addedSugars: number,
    public fiber: number,
    public protein: number,
    public sodium: number,
    public cholesterol: number,
    public vitaminA: number,
    public vitaminC: number,
    public vitaminD: number,
    public calcium: number,
    public iron: number,
    public potassium: number,
    public magnesium: number
  ) {}
}
