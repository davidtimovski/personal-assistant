-- Table: public."CookingAssistant.Shares"

-- DROP TABLE public."CookingAssistant.Shares";

CREATE TABLE public."CookingAssistant.Shares"
(
    "RecipeId" integer NOT NULL,
    "UserId" integer NOT NULL,
    "IsAccepted" boolean DEFAULT NULL,
	"LastOpenedDate" timestamp with time zone NOT NULL,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.Shares" PRIMARY KEY ("RecipeId", "UserId"),
    CONSTRAINT "FK_CookingAssistant.Shares_CookingAssistant.Recipes_RecipeId" FOREIGN KEY ("RecipeId")
    REFERENCES public."CookingAssistant.Recipes" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CookingAssistant.Shares_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.Shares"
    OWNER to personalassistant;

-- Index: IX_CookingAssistant.Shares_ListId

-- DROP INDEX public."IX_CookingAssistant.Shares_ListId";

CREATE INDEX "IX_CookingAssistant.Shares_RecipeId"
    ON public."CookingAssistant.Shares" USING btree
    ("RecipeId")
    TABLESPACE pg_default;

-- Index: IX_CookingAssistant.Shares_UserId

-- DROP INDEX public."IX_CookingAssistant.Shares_UserId";

CREATE INDEX "IX_CookingAssistant.Shares_UserId"
    ON public."CookingAssistant.Shares" USING btree
    ("UserId")
    TABLESPACE pg_default;
