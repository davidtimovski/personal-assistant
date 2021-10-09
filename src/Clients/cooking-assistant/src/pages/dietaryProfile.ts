import { inject, computedFrom, NewInstance } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import {
  ValidationController,
  validateTrigger,
  ValidationRules,
  ControllerValidateResult,
  ValidateResult,
} from "aurelia-validation";

import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { DietaryProfileService } from "services/dietaryProfileService";
import { UsersService } from "services/usersService";
import { EditDietaryProfile } from "models/viewmodels/editDietaryProfile";
import { PreferencesModel } from "models/preferencesModel";
import { UpdateDietaryProfile } from "models/viewmodels/updateDietaryProfile";
import { DailyIntake } from "models/viewmodels/dailyIntake";

@inject(
  Router,
  DietaryProfileService,
  UsersService,
  NewInstance.of(ValidationController),
  NewInstance.of(ValidationController),
  I18N,
  EventAggregator
)
export class DietaryProfile {
  private model: EditDietaryProfile;
  private originalDietaryProfileJson: string;
  private preferences: PreferencesModel;
  private dietInitiallySet = false;
  private dietTabIsVisible = false;
  private dietChanged = false;
  private minBirthdayDate: string;
  private maxBirthdayDate: string;
  private birthdayIsInvalid: boolean;
  private customCaloriesIsInvalid: boolean;
  private customSaturatedFatIsInvalid: boolean;
  private customCarbohydrateIsInvalid: boolean;
  private customAddedSugarsIsInvalid: boolean;
  private customFiberIsInvalid: boolean;
  private customProteinIsInvalid: boolean;
  private customSodiumIsInvalid: boolean;
  private customCholesterolIsInvalid: boolean;
  private customVitaminAIsInvalid: boolean;
  private customVitaminCIsInvalid: boolean;
  private customVitaminDIsInvalid: boolean;
  private customCalciumIsInvalid: boolean;
  private customIronIsInvalid: boolean;
  private customPotassiumIsInvalid: boolean;
  private customMagnesiumIsInvalid: boolean;
  private heightCmIsInvalid: boolean;
  private heightFeetIsInvalid: boolean;
  private weightKgIsInvalid: boolean;
  private weightLbsIsInvalid: boolean;
  private getRecommendedIntakeInProgress = false;
  private resetInProgress = false;
  private resetButtonText: string;
  private saveButtonIsLoading = false;
  private resetButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly dietaryProfileService: DietaryProfileService,
    private readonly usersService: UsersService,
    private readonly profileValidationController: ValidationController,
    private readonly dailyIntakeValidationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    const min = new Date();
    min.setFullYear(min.getFullYear() - 100);
    min.setMinutes(min.getMinutes() - min.getTimezoneOffset());
    this.minBirthdayDate = min.toJSON().slice(0, 10);

    const max = new Date();
    max.setFullYear(max.getFullYear() - 4);
    max.setMinutes(max.getMinutes() - max.getTimezoneOffset());
    this.maxBirthdayDate = max.toJSON().slice(0, 10);
  }

  async activate() {
    this.model = await this.dietaryProfileService.get();

    if (this.model) {
      this.dietInitiallySet = true;
      this.dietTabIsVisible = true;
    } else {
      this.model = new EditDietaryProfile(null, null, null, null, null, null, null, null, null, new DailyIntake());
    }

    this.originalDietaryProfileJson = JSON.stringify(this.model);

    this.preferences = await this.usersService.getPreferences();

    this.profileValidationController.validateTrigger = validateTrigger.manual;
    this.dailyIntakeValidationController.validateTrigger = validateTrigger.manual;
    this.resetButtonText = this.i18n.tr("dietaryProfile.reset");

    const profileValidation = ValidationRules.ensure((x: EditDietaryProfile) => x.birthday)
      .required()
      .withMessage(this.i18n.tr("dietaryProfile.birthdayRequired"))
      .ensure((x: EditDietaryProfile) => x.gender)
      .required()
      .withMessage(this.i18n.tr("dietaryProfile.genderRequired"))
      .ensure((x: EditDietaryProfile) => x.activityLevel)
      .required()
      .withMessage(this.i18n.tr("dietaryProfile.activityLevelRequired"))
      .ensure((x: EditDietaryProfile) => x.goal)
      .required()
      .withMessage(this.i18n.tr("dietaryProfile.goalRequired"));

    if (this.preferences.imperialSystem) {
      profileValidation
        .ensure((x: EditDietaryProfile) => x.heightFeet)
        .required()
        .withMessage(this.i18n.tr("dietaryProfile.heightRequired"))
        .ensure((x: EditDietaryProfile) => x.weightLbs)
        .required()
        .withMessage(this.i18n.tr("dietaryProfile.weightRequired"));
    } else {
      profileValidation
        .ensure((x: EditDietaryProfile) => x.heightCm)
        .required()
        .withMessage(this.i18n.tr("dietaryProfile.heightRequired"))
        .ensure((x: EditDietaryProfile) => x.weightKg)
        .required()
        .withMessage(this.i18n.tr("dietaryProfile.weightRequired"));
    }
    this.profileValidationController.addObject(this.model, profileValidation.rules);

    const dailyIntakeValidation = ValidationRules.ensure((x: DailyIntake) => x.customCalories)
      .between(299, 10000)
      .withMessage(this.i18n.tr("caloriesBetween", { from: 300, to: 9999 }))
      .ensure((x: DailyIntake) => x.customSaturatedFat)
      .between(0, 100)
      .withMessage(this.i18n.tr("saturatedFatBetween", { from: 1, to: 99 }))
      .ensure((x: DailyIntake) => x.customCarbohydrate)
      .between(0, 1300)
      .withMessage(this.i18n.tr("carbohydrateBetween", { from: 1, to: 1299 }))
      .ensure((x: DailyIntake) => x.customAddedSugars)
      .between(0, 100)
      .withMessage(this.i18n.tr("addedSugarsBetween", { from: 1, to: 99 }))
      .ensure((x: DailyIntake) => x.customFiber)
      .between(0, 330)
      .withMessage(this.i18n.tr("fiberBetween", { from: 1, to: 229 }))
      .ensure((x: DailyIntake) => x.customProtein)
      .between(0, 560)
      .withMessage(this.i18n.tr("proteinBetween", { from: 1, to: 559 }))
      .ensure((x: DailyIntake) => x.customSodium)
      .between(0, 23000)
      .withMessage(this.i18n.tr("sodiumBetween", { from: 1, to: 22999 }))
      .ensure((x: DailyIntake) => x.customCholesterol)
      .between(0, 3000)
      .withMessage(this.i18n.tr("cholesterolBetween", { from: 1, to: 2999 }))
      .ensure((x: DailyIntake) => x.customVitaminA)
      .between(0, 9000)
      .withMessage(this.i18n.tr("vitaminABetween", { from: 1, to: 8999 }))
      .ensure((x: DailyIntake) => x.customVitaminC)
      .between(0, 900)
      .withMessage(this.i18n.tr("vitaminCBetween", { from: 1, to: 899 }))
      .ensure((x: DailyIntake) => x.customVitaminD)
      .between(0, 6000)
      .withMessage(this.i18n.tr("vitaminDBetween", { from: 1, to: 5999 }))
      .ensure((x: DailyIntake) => x.customCalcium)
      .between(0, 10000)
      .withMessage(this.i18n.tr("calciumBetween", { from: 1, to: 9999 }))
      .ensure((x: DailyIntake) => x.customIron)
      .between(0, 80)
      .withMessage(this.i18n.tr("ironBetween", { from: 1, to: 79 }))
      .ensure((x: DailyIntake) => x.customPotassium)
      .between(0, 32000)
      .withMessage(this.i18n.tr("potassiumBetween", { from: 1, to: 22999 }))
      .ensure((x: DailyIntake) => x.customMagnesium)
      .between(0, 4000)
      .withMessage(this.i18n.tr("magnesiumBetween", { from: 1, to: 3999 }));
    this.dailyIntakeValidationController.addObject(this.model.dailyIntake, dailyIntakeValidation.rules);
  }

  viewProfileTab() {
    this.dietTabIsVisible = false;
  }

  viewDietTab() {
    this.dietTabIsVisible = true;
    this.dietChanged = false;
  }

  async getAndUpdateRecommendedIntake() {
    const result: ControllerValidateResult = await this.profileValidationController.validate();

    if (result.valid) {
      this.getRecommendedIntakeInProgress = true;

      const recommended = await this.dietaryProfileService.getDailyIntake(this.model);

      this.model.dailyIntake.calories = recommended.calories;
      //this.dietaryProfile.dailyIntake.fat = recommended.fat;
      this.model.dailyIntake.saturatedFat = recommended.saturatedFat;
      this.model.dailyIntake.carbohydrate = recommended.carbohydrate;
      this.model.dailyIntake.addedSugars = recommended.addedSugars;
      this.model.dailyIntake.fiber = recommended.fiber;
      this.model.dailyIntake.protein = recommended.protein;
      this.model.dailyIntake.sodium = recommended.sodium;
      this.model.dailyIntake.cholesterol = recommended.cholesterol;
      this.model.dailyIntake.vitaminA = recommended.vitaminA;
      this.model.dailyIntake.vitaminC = recommended.vitaminC;
      this.model.dailyIntake.vitaminD = recommended.vitaminD;
      this.model.dailyIntake.calcium = recommended.calcium;
      this.model.dailyIntake.iron = recommended.iron;
      this.model.dailyIntake.potassium = recommended.potassium;
      this.model.dailyIntake.magnesium = recommended.magnesium;

      if (!this.dietInitiallySet) {
        this.model.dailyIntake.trackCalories = true;
        //this.dietaryProfile.dailyIntake.trackFat = true;
        this.model.dailyIntake.trackSaturatedFat = true;
        this.model.dailyIntake.trackCarbohydrate = true;
        this.model.dailyIntake.trackAddedSugars = true;
        this.model.dailyIntake.trackFiber = true;
        this.model.dailyIntake.trackProtein = true;
        this.model.dailyIntake.trackSodium = true;
        this.model.dailyIntake.trackCholesterol = true;
        this.model.dailyIntake.trackVitaminA = true;
        this.model.dailyIntake.trackVitaminC = true;
        this.model.dailyIntake.trackVitaminD = true;
        this.model.dailyIntake.trackCalcium = true;
        this.model.dailyIntake.trackIron = true;
        this.model.dailyIntake.trackPotassium = true;
        this.model.dailyIntake.trackMagnesium = true;
      }

      this.getRecommendedIntakeInProgress = false;
      this.dietChanged = true;
    }
  }

  @computedFrom(
    "model.birthday",
    "model.gender",
    "model.heightCm",
    "model.heightFeet",
    "model.heightInches",
    "model.weightKg",
    "model.weightLbs",
    "model.activityLevel",
    "model.goal"
  )
  get dietaryProfileIsSet() {
    let valid = !!this.model.birthday && !!this.model.gender && !!this.model.activityLevel && !!this.model.goal;

    if (this.preferences.imperialSystem) {
      valid = valid && !!this.model.heightFeet && !!this.model.weightLbs;
    } else {
      valid = valid && !!this.model.heightCm && !!this.model.weightKg;
    }

    return valid;
  }

  @computedFrom(
    "model.birthday",
    "model.gender",
    "model.heightCm",
    "model.heightFeet",
    "model.heightInches",
    "model.weightKg",
    "model.weightLbs",
    "model.activityLevel",
    "model.goal",
    "model.dailyIntake.customCalories",
    "model.dailyIntake.trackCalories",
    "model.dailyIntake.customSaturatedFat",
    "model.dailyIntake.trackSaturatedFat",
    "model.dailyIntake.customCarbohydrate",
    "model.dailyIntake.trackCarbohydrate",
    "model.dailyIntake.customAddedSugars",
    "model.dailyIntake.trackAddedSugars",
    "model.dailyIntake.customFiber",
    "model.dailyIntake.trackFiber",
    "model.dailyIntake.customProtein",
    "model.dailyIntake.trackProtein",
    "model.dailyIntake.customSodium",
    "model.dailyIntake.trackSodium",
    "model.dailyIntake.customCholesterol",
    "model.dailyIntake.trackCholesterol",
    "model.dailyIntake.customVitaminA",
    "model.dailyIntake.trackVitaminA",
    "model.dailyIntake.customVitaminC",
    "model.dailyIntake.trackVitaminC",
    "model.dailyIntake.customVitaminD",
    "model.dailyIntake.trackVitaminD",
    "model.dailyIntake.customCalcium",
    "model.dailyIntake.trackCalcium",
    "model.dailyIntake.customIron",
    "model.dailyIntake.trackIron",
    "model.dailyIntake.customPotassium",
    "model.dailyIntake.trackPotassium",
    "model.dailyIntake.customMagnesium",
    "model.dailyIntake.trackMagnesium"
  )
  get canSave() {
    let valid = !!this.model.birthday && !!this.model.gender && !!this.model.activityLevel && !!this.model.goal;

    if (this.preferences.imperialSystem) {
      valid = valid && !!this.model.heightFeet && !!this.model.weightLbs;
    } else {
      valid = valid && !!this.model.heightCm && !!this.model.weightKg;
    }

    return valid && JSON.stringify(this.model) !== this.originalDietaryProfileJson;
  }

  propertyIsInvalid(properties: ValidateResult[], property: string, errorMessages: Array<string>): boolean {
    let invalidProperty = properties.find((p) => {
      return p.propertyName === property && !p.valid;
    });

    if (invalidProperty) {
      errorMessages.push(invalidProperty.message);
      return true;
    }
    return false;
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const profileResult: ControllerValidateResult = await this.profileValidationController.validate();
    const dailyIntakeResult: ControllerValidateResult = await this.dailyIntakeValidationController.validate();

    if (profileResult.valid && dailyIntakeResult.valid) {
      try {
        await this.dietaryProfileService.update(this.toUpdateModel());

        this.birthdayIsInvalid =
          this.heightCmIsInvalid =
          this.heightFeetIsInvalid =
          this.weightKgIsInvalid =
          this.weightLbsIsInvalid =
            false;

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "dietaryProfile.updateSuccessful");
        this.router.navigateToRoute("menu");
      } catch {
        this.saveButtonIsLoading = false;
      }
    } else {
      let errorMessages = new Array<string>();

      this.birthdayIsInvalid = this.propertyIsInvalid(profileResult.results, "birthday", errorMessages);
      this.customCaloriesIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customCalories", errorMessages);
      this.customSaturatedFatIsInvalid = this.propertyIsInvalid(
        dailyIntakeResult.results,
        "customSaturatedFat",
        errorMessages
      );
      this.customCarbohydrateIsInvalid = this.propertyIsInvalid(
        dailyIntakeResult.results,
        "customCarbohydrate",
        errorMessages
      );
      this.customAddedSugarsIsInvalid = this.propertyIsInvalid(
        dailyIntakeResult.results,
        "customAddedSugars",
        errorMessages
      );
      this.customFiberIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customFiber", errorMessages);
      this.customProteinIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customProtein", errorMessages);
      this.customSodiumIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customSodium", errorMessages);
      this.customCholesterolIsInvalid = this.propertyIsInvalid(
        dailyIntakeResult.results,
        "customCholesterol",
        errorMessages
      );
      this.customVitaminAIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customVitaminA", errorMessages);
      this.customVitaminCIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customVitaminC", errorMessages);
      this.customVitaminDIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customVitaminD", errorMessages);
      this.customCalciumIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customCalcium", errorMessages);
      this.customIronIsInvalid = this.propertyIsInvalid(dailyIntakeResult.results, "customIron", errorMessages);
      this.customPotassiumIsInvalid = this.propertyIsInvalid(
        dailyIntakeResult.results,
        "customPotassium",
        errorMessages
      );
      this.customMagnesiumIsInvalid = this.propertyIsInvalid(
        dailyIntakeResult.results,
        "customMagnesium",
        errorMessages
      );
      this.heightCmIsInvalid = this.propertyIsInvalid(profileResult.results, "heightCm", errorMessages);
      this.heightFeetIsInvalid = this.propertyIsInvalid(profileResult.results, "heightFeet", errorMessages);
      this.weightKgIsInvalid = this.propertyIsInvalid(profileResult.results, "weightKg", errorMessages);
      this.weightLbsIsInvalid = this.propertyIsInvalid(profileResult.results, "weightLbs", errorMessages);

      this.eventAggregator.publish(AlertEvents.ShowError, errorMessages);
    }

    this.saveButtonIsLoading = false;
  }

  returnIfAdjusted(custom: number, recommended: number) {
    if (!custom) {
      return null;
    }

    const diff = custom - recommended;

    return diff === 0 ? null : custom;
  }

  toUpdateModel(): UpdateDietaryProfile {
    return new UpdateDietaryProfile(
      this.model.birthday,
      this.model.gender,
      this.model.heightCm,
      this.model.heightFeet,
      this.model.heightInches,
      this.model.weightKg,
      this.model.weightLbs,
      this.model.activityLevel,
      this.model.goal,
      this.returnIfAdjusted(this.model.dailyIntake.customCalories, this.model.dailyIntake.calories),
      this.model.dailyIntake.trackCalories,
      this.returnIfAdjusted(this.model.dailyIntake.customSaturatedFat, this.model.dailyIntake.saturatedFat),
      this.model.dailyIntake.trackSaturatedFat,
      this.returnIfAdjusted(this.model.dailyIntake.customCarbohydrate, this.model.dailyIntake.carbohydrate),
      this.model.dailyIntake.trackCarbohydrate,
      this.returnIfAdjusted(this.model.dailyIntake.customAddedSugars, this.model.dailyIntake.addedSugars),
      this.model.dailyIntake.trackAddedSugars,
      this.returnIfAdjusted(this.model.dailyIntake.customFiber, this.model.dailyIntake.fiber),
      this.model.dailyIntake.trackFiber,
      this.returnIfAdjusted(this.model.dailyIntake.customProtein, this.model.dailyIntake.protein),
      this.model.dailyIntake.trackProtein,
      this.returnIfAdjusted(this.model.dailyIntake.customSodium, this.model.dailyIntake.sodium),
      this.model.dailyIntake.trackSodium,
      this.returnIfAdjusted(this.model.dailyIntake.customCholesterol, this.model.dailyIntake.cholesterol),
      this.model.dailyIntake.trackCholesterol,
      this.returnIfAdjusted(this.model.dailyIntake.customVitaminA, this.model.dailyIntake.vitaminA),
      this.model.dailyIntake.trackVitaminA,
      this.returnIfAdjusted(this.model.dailyIntake.customVitaminC, this.model.dailyIntake.vitaminC),
      this.model.dailyIntake.trackVitaminC,
      this.returnIfAdjusted(this.model.dailyIntake.customVitaminD, this.model.dailyIntake.vitaminD),
      this.model.dailyIntake.trackVitaminD,
      this.returnIfAdjusted(this.model.dailyIntake.customCalcium, this.model.dailyIntake.calcium),
      this.model.dailyIntake.trackCalcium,
      this.returnIfAdjusted(this.model.dailyIntake.customIron, this.model.dailyIntake.iron),
      this.model.dailyIntake.trackIron,
      this.returnIfAdjusted(this.model.dailyIntake.customPotassium, this.model.dailyIntake.potassium),
      this.model.dailyIntake.trackPotassium,
      this.returnIfAdjusted(this.model.dailyIntake.customMagnesium, this.model.dailyIntake.magnesium),
      this.model.dailyIntake.trackMagnesium
    );
  }

  async reset() {
    if (this.resetButtonIsLoading) {
      return;
    }

    if (this.resetInProgress) {
      this.resetButtonIsLoading = true;

      await this.dietaryProfileService.delete();
      this.eventAggregator.publish(AlertEvents.ShowSuccess, "dietaryProfile.resetSuccessful");
      this.router.navigateToRoute("menu");
    } else {
      this.resetButtonText = this.i18n.tr("sure");
      this.resetInProgress = true;
    }
  }

  cancel() {
    this.resetButtonText = this.i18n.tr("dietaryProfile.reset");
    this.resetInProgress = false;
  }
}
