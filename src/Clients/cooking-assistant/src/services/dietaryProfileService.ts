import { json } from "aurelia-fetch-client";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";

import { EditDietaryProfile } from "../models/viewmodels/editDietaryProfile";
import { UpdateDietaryProfile } from "../models/viewmodels/updateDietaryProfile";
import { RecommendedDailyIntake } from "../models/viewmodels/recommendedDailyIntake";

export class DietaryProfileService extends HttpProxyBase {
  async get(): Promise<EditDietaryProfile> {
    const result = await this.ajax<EditDietaryProfile>("dietaryprofiles");
    return result;
  }

  async getDailyIntake(getRecommendedIntake: EditDietaryProfile): Promise<RecommendedDailyIntake> {
    const result = await this.ajax<RecommendedDailyIntake>("dietaryprofiles", {
      method: "post",
      body: json({
        birthday: getRecommendedIntake.birthday,
        gender: getRecommendedIntake.gender,
        heightCm: getRecommendedIntake.heightCm,
        heightFeet: getRecommendedIntake.heightFeet,
        heightInches: getRecommendedIntake.heightInches,
        weightKg: getRecommendedIntake.weightKg,
        weightLbs: getRecommendedIntake.weightLbs,
        activityLevel: getRecommendedIntake.activityLevel,
        goal: getRecommendedIntake.goal,
      }),
    });
    return result;
  }

  async update(updateDietaryProfile: UpdateDietaryProfile): Promise<void> {
    await this.ajaxExecute("dietaryprofiles", {
      method: "put",
      body: json(updateDietaryProfile),
    });
  }

  async delete(): Promise<void> {
    await this.ajaxExecute("dietaryprofiles", {
      method: "delete",
    });
  }
}
