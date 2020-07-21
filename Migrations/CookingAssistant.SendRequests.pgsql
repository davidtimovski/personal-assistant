-- Table: public."CookingAssistant.SendRequests"

-- DROP TABLE public."CookingAssistant.SendRequests";

CREATE TABLE public."CookingAssistant.SendRequests"
(
    "RecipeId" integer NOT NULL,
    "UserId" integer NOT NULL,
    "IsDeclined" boolean NOT NULL DEFAULT FALSE,
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.SendRequests" PRIMARY KEY ("RecipeId", "UserId"),
    CONSTRAINT "FK_CA.SendRequests_CA.Recipes_RecipeId" FOREIGN KEY ("RecipeId")
    REFERENCES public."CookingAssistant.Recipes" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CookingAssistant.SendRequests_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.SendRequests"
    OWNER to personalassistant;

-- Index: IX_CookingAssistant.SendRequests_RecipeId

-- DROP INDEX public."IX_CookingAssistant.SendRequests_RecipeId";

CREATE INDEX "IX_CookingAssistant.SendRequests_RecipeId"
    ON public."CookingAssistant.SendRequests" USING btree
    ("RecipeId")
    TABLESPACE pg_default;

-- Index: IX_CookingAssistant.SendRequests_UserId

-- DROP INDEX public."IX_CookingAssistant.SendRequests_UserId";

CREATE INDEX "IX_CookingAssistant.SendRequests_UserId"
    ON public."CookingAssistant.SendRequests" USING btree
    ("UserId")
    TABLESPACE pg_default;