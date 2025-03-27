INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Chapter 1: Logic', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Chapter 2: Science', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Chapter 3: Perversions of Science', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Chapter 4: Health', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Chapter 5: Economics', 'FK_Excerpt');

DELETE 
FROM blog_post_tag_lookup
WHERE id = 4;

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Nutrition', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Statins', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Dentistry', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Sunlight', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Cancer', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Vaccines, Germs, and Terrain', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Psychiatry', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Birth', 'FK_Excerpt');

ALTER TABLE IF EXISTS public.blog_post_tag_lookup
ADD chapter int;

UPDATE blog_post_tag_lookup
SET chapter = '4'
WHERE id >= 10 AND id <= 17;

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Basic Economics', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Property, Ideas, and Monopoly', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Money, Prices, and Calculation', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Banking and Business Cycles', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Economics of Science', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Economics of Education', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Economics of Healthcare', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('The Free Market, Corporatism, and Socialism', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Sweatshops, Child Labor, and Labor Politics', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('The Production of Security', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('War', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('State Debt and Government Pensions', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Class Theory', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Democracy', 'FK_Excerpt');

INSERT INTO public.blog_post_tag_lookup(name, tag_type)
VALUES ('Economics of Land', 'FK_Excerpt');

UPDATE blog_post_tag_lookup
SET chapter = '5'
WHERE id > 17;