import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { EditDietaryProfile } from "models/viewmodels/editDietaryProfile";
import { UpdateDietaryProfile } from "models/viewmodels/updateDietaryProfile";
import { RecommendedDailyIntake } from "models/viewmodels/recommendedDailyIntake";

@autoinject
export class DietaryProfileService {
  constructor(private readonly httpProxy: HttpProxy, private readonly logger: ErrorLogger) {}

  get(): Promise<EditDietaryProfile> {
    return this.httpProxy.ajax<EditDietaryProfile>("api/dietaryprofiles");
  }

  async getDailyIntake(getRecommendedIntake: EditDietaryProfile): Promise<RecommendedDailyIntake> {
    const result = await this.httpProxy.ajax<RecommendedDailyIntake>("api/dietaryprofiles", {
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
    try {
      await this.httpProxy.ajaxExecute("api/dietaryprofiles", {
        method: "put",
        body: json(updateDietaryProfile),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/dietaryprofiles", {
        method: "delete",
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
