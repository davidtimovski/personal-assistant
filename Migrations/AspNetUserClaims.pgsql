-- Table: public."AspNetUserClaims"

-- DROP TABLE public."AspNetUserClaims";

CREATE TABLE public."AspNetUserClaims"
(
    "Id" serial NOT NULL,
    "ClaimType" text COLLATE pg_catalog."default",
    "ClaimValue" text COLLATE pg_catalog."default",
    "UserId" integer NOT NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId")
        REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."AspNetUserClaims"
    OWNER to personalassistant;

-- Index: IX_AspNetUserClaims_UserId

-- DROP INDEX public."IX_AspNetUserClaims_UserId";

CREATE INDEX "IX_AspNetUserClaims_UserId"
    ON public."AspNetUserClaims" USING btree
    ("UserId")
    TABLESPACE pg_default;