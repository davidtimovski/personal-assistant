import { json } from "aurelia-fetch-client";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { EditDietaryProfile } from "../models/viewmodels/editDietaryProfile";
import { UpdateDietaryProfile } from "../models/viewmodels/updateDietaryProfile";
import { RecommendedDailyIntake } from "../models/viewmodels/recommendedDailyIntake";
import * as environment from "../../config/environment.json";

export class DietaryProfileService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  get(): Promise<EditDietaryProfile> {
    return this.ajax<EditDietaryProfile>("dietaryprofiles");
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
    try {
      await this.ajaxExecute("dietaryprofiles", {
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
      await this.ajaxExecute("dietaryprofiles", {
        method: "delete",
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
