-- Table: public."CookingAssistant.DietaryProfiles"

-- DROP TABLE public."CookingAssistant.DietaryProfiles";

CREATE TABLE public."CookingAssistant.DietaryProfiles"
(
    "UserId" integer NOT NULL,
    "Birthday" date,
	"Gender" character varying(6),
	"Height" numeric(4, 1),
	"Weight" numeric(6, 3),
	"ActivityLevel" character varying(10),
	"Goal" character varying(14) NOT NULL DEFAULT 'None',
	"CustomCalories" smallint,
	"TrackCalories" boolean NOT NULL DEFAULT TRUE,
	"CustomSaturatedFat" smallint,
	"TrackSaturatedFat" boolean NOT NULL DEFAULT TRUE,
	"CustomCarbohydrate" smallint,
	"TrackCarbohydrate" boolean NOT NULL DEFAULT TRUE,
	"CustomAddedSugars" smallint,
	"TrackAddedSugars" boolean NOT NULL DEFAULT TRUE,
	"CustomFiber" numeric(4, 1),
	"TrackFiber" boolean NOT NULL DEFAULT TRUE,
	"CustomProtein" smallint,
	"TrackProtein" boolean NOT NULL DEFAULT TRUE,
	"CustomSodium" smallint,
	"TrackSodium" boolean NOT NULL DEFAULT TRUE,
	"CustomCholesterol" smallint,
	"TrackCholesterol" boolean NOT NULL DEFAULT TRUE,
	"CustomVitaminA" smallint,
	"TrackVitaminA" boolean NOT NULL DEFAULT TRUE,
	"CustomVitaminC" smallint,
	"TrackVitaminC" boolean NOT NULL DEFAULT TRUE,
	"CustomVitaminD" smallint,
	"TrackVitaminD" boolean NOT NULL DEFAULT TRUE,
	"CustomCalcium" smallint,
	"TrackCalcium" boolean NOT NULL DEFAULT TRUE,
	"CustomIron" smallint,
	"TrackIron" boolean NOT NULL DEFAULT TRUE,
	"CustomPotassium" smallint,
	"TrackPotassium" boolean NOT NULL DEFAULT TRUE,
	"CustomMagnesium" smallint,
	"TrackMagnesium" boolean NOT NULL DEFAULT TRUE,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.DietaryProfiles" PRIMARY KEY ("UserId"),
    CONSTRAINT "FK_CookingAssistant.DietaryProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.DietaryProfiles"
    OWNER to personalassistant;

-- Index: IX_CookingAssistant.DietaryProfiles_UserId

-- DROP INDEX public."IX_CookingAssistant.DietaryProfiles_UserId";

CREATE INDEX "IX_CookingAssistant.DietaryProfiles_UserId"
    ON public."CookingAssistant.DietaryProfiles" USING btree
    ("UserId")
    TABLESPACE pg_default;
