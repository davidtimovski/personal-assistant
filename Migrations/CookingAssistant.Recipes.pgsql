-- Table: public."CookingAssistant.Recipes"

-- DROP TABLE public."CookingAssistant.Recipes";

CREATE TABLE public."CookingAssistant.Recipes"
(
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL,
    "Name" character varying(50) NOT NULL COLLATE pg_catalog."default",
    "Description" character varying(255) COLLATE pg_catalog."default",
    "Instructions" character varying(5000) COLLATE pg_catalog."default",
    "PrepDuration" interval(0),
    "CookDuration" interval(0),
    "Servings" smallint NOT NULL,
    "ImageUri" character varying(255) NOT NULL COLLATE pg_catalog."default",
    "VideoSrc" character varying(1024) COLLATE pg_catalog."default",
    "LastOpenedDate" timestamp without time zone NOT NULL,
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.Recipes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CookingAssistant.Recipes_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.Recipes"
    OWNER to personalassistant;

-- Index: IX_CookingAssistant.Recipes_UserId

-- DROP INDEX public."IX_CookingAssistant.Recipes_UserId";

CREATE INDEX "IX_CookingAssistant.Recipes_UserId"
    ON public."CookingAssistant.Recipes" USING btree
    ("UserId")
    TABLESPACE pg_default;