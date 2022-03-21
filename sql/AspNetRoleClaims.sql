-- Table: public."AspNetRoleClaims"

-- DROP TABLE public."AspNetRoleClaims";

CREATE TABLE public."AspNetRoleClaims"
(
    "Id" serial NOT NULL,
    "ClaimType" text COLLATE pg_catalog."default",
    "ClaimValue" text COLLATE pg_catalog."default",
    "RoleId" integer NOT NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId")
        REFERENCES public."AspNetRoles" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."AspNetRoleClaims"
    OWNER to personalassistant;

-- Index: IX_AspNetRoleClaims_RoleId

-- DROP INDEX public."IX_AspNetRoleClaims_RoleId";

CREATE INDEX "IX_AspNetRoleClaims_RoleId"
    ON public."AspNetRoleClaims" USING btree
    ("RoleId")
    TABLESPACE pg_default;
