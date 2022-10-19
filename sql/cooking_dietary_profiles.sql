CREATE TABLE public.cooking_dietary_profiles
(
    user_id integer NOT NULL,
    birthday date,
    gender character varying(6),
    height numeric(4, 1),
    weight numeric(6, 3),
    activity_level character varying(10),
    goal character varying(14) NOT NULL DEFAULT 'None',
    custom_calories smallint,
    track_calories boolean NOT NULL DEFAULT TRUE,
    custom_saturated_fat smallint,
    track_saturated_fat boolean NOT NULL DEFAULT TRUE,
    custom_carbohydrate smallint,
    track_carbohydrate boolean NOT NULL DEFAULT TRUE,
    custom_added_sugars smallint,
    track_added_sugars boolean NOT NULL DEFAULT TRUE,
    custom_fiber numeric(4, 1),
    track_fiber boolean NOT NULL DEFAULT TRUE,
    custom_protein smallint,
    track_protein boolean NOT NULL DEFAULT TRUE,
    custom_sodium smallint,
    track_sodium boolean NOT NULL DEFAULT TRUE,
    custom_cholesterol smallint,
    track_cholesterol boolean NOT NULL DEFAULT TRUE,
    custom_vitamin_a smallint,
    track_vitamin_a boolean NOT NULL DEFAULT TRUE,
    custom_vitamin_c smallint,
    track_vitamin_c boolean NOT NULL DEFAULT TRUE,
    custom_vitamin_d smallint,
    track_vitamin_d boolean NOT NULL DEFAULT TRUE,
    custom_calcium smallint,
    track_calcium boolean NOT NULL DEFAULT TRUE,
    custom_iron smallint,
    track_iron boolean NOT NULL DEFAULT TRUE,
    custom_potassium smallint,
    track_potassium boolean NOT NULL DEFAULT TRUE,
    custom_magnesium smallint,
    track_magnesium boolean NOT NULL DEFAULT TRUE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_cooking_dietary_profiles PRIMARY KEY (user_id),
    CONSTRAINT fk_cooking_dietary_profiles_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.cooking_dietary_profiles
    OWNER to personalassistant;

CREATE INDEX ix_cooking_dietary_profiles_user_id
    ON public.cooking_dietary_profiles USING btree
    (user_id)
    TABLESPACE pg_default;
