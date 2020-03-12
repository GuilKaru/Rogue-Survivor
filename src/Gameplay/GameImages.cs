using Microsoft.Xna.Framework.Graphics;
using RogueSurvivor.Extensions;
using RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using Xna = Microsoft.Xna.Framework;

namespace RogueSurvivor.Gameplay
{
    static class GameImages
    {
        const float GRAYLEVEL_DIM_FACTOR = 0.55f;
        const string FOLDER = @"Resources\Images\";

        public const string ACTIVITY_CHASING = @"Activities\chasing";
        public const string ACTIVITY_CHASING_PLAYER = @"Activities\chasing_player";
        public const string ACTIVITY_TRACKING = @"Activities\tracking";
        public const string ACTIVITY_FLEEING = @"Activities\fleeing";
        public const string ACTIVITY_FLEEING_FROM_EXPLOSIVE = @"Activities\fleeing_explosive";
        public const string ACTIVITY_FOLLOWING = @"Activities\following";
        public const string ACTIVITY_FOLLOWING_ORDER = @"Activities\following_order";
        public const string ACTIVITY_FOLLOWING_PLAYER = @"Activities\following_player";
        public const string ACTIVITY_FOLLOWING_LEADER = @"Activities\following_leader";
        public const string ACTIVITY_SLEEPING = @"Activities\sleeping";

        public const string ICON_BLAST = @"Icons\blast";
        public const string ICON_CAN_TRADE = @"Icons\can_trade";
        public const string ICON_HAS_VITAL_ITEM = @"Icons\has_vital_item";
        public const string ICON_THREAT_SAFE = @"Icons\threat_safe";
        public const string ICON_THREAT_DANGER = @"Icons\threat_danger";
        public const string ICON_THREAT_HIGH_DANGER = @"Icons\threat_high_danger";
        public const string ICON_CANT_RUN = @"Icons\cant_run";
        public const string ICON_EXPIRED_FOOD = @"Icons\expired_food";
        public const string ICON_FOOD_ALMOST_HUNGRY = @"Icons\food_almost_hungry";
        public const string ICON_FOOD_HUNGRY = @"Icons\food_hungry";
        public const string ICON_FOOD_STARVING = @"Icons\food_starving";
        public const string ICON_HEALING = @"Icons\healing";
        public const string ICON_IS_TARGET = @"Icons\is_target";
        public const string ICON_IS_TARGETTED = @"Icons\is_targetted";
        public const string ICON_IS_TARGETING = @"Icons\is_targeting";
        public const string ICON_IS_IN_GROUP = @"Icons\is_in_group";
        public const string ICON_KILLED = @"Icons\killed";
        public const string ICON_LEADER = @"Icons\leader";
        public const string ICON_MELEE_ATTACK = @"Icons\melee_attack";
        public const string ICON_MELEE_MISS = @"Icons\melee_miss";
        public const string ICON_MELEE_DAMAGE = @"Icons\melee_damage";
        public const string ICON_ODOR_SUPPRESSED = @"Icons\odor_suppressed";
        public const string ICON_OUT_OF_AMMO = @"Icons\out_of_ammo";
        public const string ICON_OUT_OF_BATTERIES = @"Icons\out_of_batteries";
        public const string ICON_RANGED_ATTACK = @"Icons\ranged_attack";
        public const string ICON_RANGED_MISS = @"Icons\ranged_miss";
        public const string ICON_RANGED_DAMAGE = @"Icons\ranged_damage";
        public const string ICON_RUNNING = @"Icons\running";
        public const string ICON_ROT_ALMOST_HUNGRY = @"Icons\rot_almost_hungry";
        public const string ICON_ROT_HUNGRY = @"Icons\rot_hungry";
        public const string ICON_ROT_STARVING = @"Icons\rot_starving";
        public const string ICON_SLEEP_ALMOST_SLEEPY = @"Icons\sleep_almost_sleepy";
        public const string ICON_SLEEP_EXHAUSTED = @"Icons\sleep_exhausted";
        public const string ICON_SLEEP_SLEEPY = @"Icons\sleep_sleepy";
        public const string ICON_SPOILED_FOOD = @"Icons\spoiled_food";
        public const string ICON_TARGET = @"Icons\target";
        public const string ICON_LINE_BLOCKED = @"Icons\line_blocked";
        public const string ICON_LINE_CLEAR = @"Icons\line_clear";
        public const string ICON_LINE_BAD = @"Icons\line_bad";
        public const string ICON_SCENT_LIVING = @"Icons\scent_living";
        public const string ICON_SCENT_ZOMBIEMASTER = @"Icons\scent_zm";
        public const string ICON_AGGRESSOR = @"Icons\enemy_you_aggressor";
        public const string ICON_INDIRECT_ENEMIES = @"Icons\enemy_indirect";
        public const string ICON_SELF_DEFENCE = @"Icons\enemy_you_self_defence";
        public const string ICON_TRAP_ACTIVATED = @"Icons\trap_activated";
        public const string ICON_TRAP_ACTIVATED_SAFE_GROUP = @"Icons\trap_activated_safe_group";
        public const string ICON_TRAP_ACTIVATED_SAFE_PLAYER = @"Icons\trap_activated_safe_player";
        public const string ICON_TRAP_TRIGGERED = @"Icons\trap_triggered";
        public const string ICON_TRAP_TRIGGERED_SAFE_GROUP = @"Icons\trap_triggered_safe_group";
        public const string ICON_TRAP_TRIGGERED_SAFE_PLAYER = @"Icons\trap_triggered_safe_player";
        public const string ICON_SANITY_DISTURBED = @"Icons\sanity_disturbed";
        public const string ICON_SANITY_INSANE = @"Icons\sanity_insane";
        public const string ICON_BORING_ITEM = @"Icons\boring_item";
        public const string ICON_ZGRAB = @"Icons\zgrab";

        public const string TILE_FLOOR_ASPHALT = @"Tiles\floor_asphalt";
        public const string TILE_FLOOR_CONCRETE = @"Tiles\floor_concrete";
        public const string TILE_FLOOR_GRASS = @"Tiles\floor_grass";
        public const string TILE_FLOOR_OFFICE = @"Tiles\floor_office";
        public const string TILE_FLOOR_PLANKS = @"Tiles\floor_planks";
        public const string TILE_FLOOR_SEWER_WATER = @"Tiles\floor_sewer_water";
        public const string TILE_FLOOR_SEWER_WATER_ANIM1 = @"Tiles\floor_sewer_water_anim1";
        public const string TILE_FLOOR_SEWER_WATER_ANIM2 = @"Tiles\floor_sewer_water_anim2";
        public const string TILE_FLOOR_SEWER_WATER_ANIM3 = @"Tiles\floor_sewer_water_anim3";
        public const string TILE_FLOOR_SEWER_WATER_COVER = @"Tiles\floor_sewer_water_cover";
        public const string TILE_FLOOR_TILES = @"Tiles\floor_tiles";
        public const string TILE_FLOOR_WALKWAY = @"Tiles\floor_walkway";

        public const string TILE_ROAD_ASPHALT_NS = @"Tiles\road_asphalt_ns";
        public const string TILE_ROAD_ASPHALT_EW = @"Tiles\road_asphalt_ew";
        public const string TILE_RAIL_ES = @"Tiles\rail_ew";

        public const string TILE_WALL_BRICK = @"Tiles\wall_brick";
        public const string TILE_WALL_CHAR_OFFICE = @"Tiles\wall_char_office";
        public const string TILE_WALL_HOSPITAL = @"Tiles\wall_hospital";
        public const string TILE_WALL_SEWER = @"Tiles\wall_sewer";
        public const string TILE_WALL_STONE = @"Tiles\wall_stone";

        public const string DECO_BLOODIED_FLOOR = @"Tiles\Decoration\bloodied_floor";
        public const string DECO_BLOODIED_WALL = @"Tiles\Decoration\bloodied_wall";
        public const string DECO_ZOMBIE_REMAINS = @"Tiles\Decoration\zombie_remains";
        public const string DECO_VOMIT = @"Tiles\Decoration\vomit";

        public const string DECO_POSTERS1 = @"Tiles\Decoration\posters1";
        public const string DECO_POSTERS2 = @"Tiles\Decoration\posters2";
        public const string DECO_TAGS1 = @"Tiles\Decoration\tags1";
        public const string DECO_TAGS2 = @"Tiles\Decoration\tags2";
        public const string DECO_TAGS3 = @"Tiles\Decoration\tags3";
        public const string DECO_TAGS4 = @"Tiles\Decoration\tags4";
        public const string DECO_TAGS5 = @"Tiles\Decoration\tags5";
        public const string DECO_TAGS6 = @"Tiles\Decoration\tags6";
        public const string DECO_TAGS7 = @"Tiles\Decoration\tags7";

        public const string DECO_SHOP_CONSTRUCTION = @"Tiles\Decoration\shop_construction";
        public const string DECO_SHOP_GENERAL_STORE = @"Tiles\Decoration\shop_general_store";
        public const string DECO_SHOP_GROCERY = @"Tiles\Decoration\shop_grocery";
        public const string DECO_SHOP_GUNSHOP = @"Tiles\Decoration\shop_gunshop";
        public const string DECO_SHOP_PHARMACY = @"Tiles\Decoration\shop_pharmacy";
        public const string DECO_SHOP_SPORTSWEAR = @"Tiles\Decoration\shop_sportswear";
        public const string DECO_SHOP_HUNTING = @"Tiles\Decoration\shop_hunting";

        public const string DECO_CHAR_OFFICE = @"Tiles\Decoration\char_office";
        public const string DECO_CHAR_FLOOR_LOGO = @"Tiles\Decoration\char_floor_logo";
        public const string DECO_CHAR_POSTER1 = @"Tiles\Decoration\char_poster1";
        public const string DECO_CHAR_POSTER2 = @"Tiles\Decoration\char_poster2";
        public const string DECO_CHAR_POSTER3 = @"Tiles\Decoration\char_poster3";

        public const string DECO_PLAYER_TAG1 = @"Tiles\Decoration\player_tag";
        public const string DECO_PLAYER_TAG2 = @"Tiles\Decoration\player_tag2";
        public const string DECO_PLAYER_TAG3 = @"Tiles\Decoration\player_tag3";
        public const string DECO_PLAYER_TAG4 = @"Tiles\Decoration\player_tag4";

        public const string DECO_ROGUEDJACK_TAG = @"Tiles\Decoration\roguedjack";

        public const string DECO_SEWER_LADDER = @"Tiles\Decoration\sewer_ladder";
        public const string DECO_SEWER_HOLE = @"Tiles\Decoration\sewer_hole";
        public const string DECO_SEWERS_BUILDING = @"Tiles\Decoration\sewers_building";

        public const string DECO_SUBWAY_BUILDING = @"Tiles\Decoration\subway_building";

        public const string DECO_STAIRS_UP = @"Tiles\Decoration\stairs_up";
        public const string DECO_STAIRS_DOWN = @"Tiles\Decoration\stairs_down";

        public const string DECO_POWER_SIGN_BIG = @"Tiles\Decoration\power_sign_big";

        public const string DECO_POLICE_STATION = @"Tiles\Decoration\police_station";

        public const string DECO_HOSPITAL = @"Tiles\Decoration\hospital";

        public const string OBJ_TREE = @"MapObjects\tree";

        public const string OBJ_WOODEN_DOOR_CLOSED = @"MapObjects\wooden_door_closed";
        public const string OBJ_WOODEN_DOOR_OPEN = @"MapObjects\wooden_door_open";
        public const string OBJ_WOODEN_DOOR_BROKEN = @"MapObjects\wooden_door_broken";

        public const string OBJ_GLASS_DOOR_CLOSED = @"MapObjects\glass_door_closed";
        public const string OBJ_GLASS_DOOR_OPEN = @"MapObjects\glass_door_open";
        public const string OBJ_GLASS_DOOR_BROKEN = @"MapObjects\glass_door_broken";

        public const string OBJ_CHAR_DOOR_CLOSED = @"MapObjects\dark_door_closed";
        public const string OBJ_CHAR_DOOR_OPEN = @"MapObjects\dark_door_open";
        public const string OBJ_CHAR_DOOR_BROKEN = @"MapObjects\dark_door_broken";

        public const string OBJ_WINDOW_CLOSED = @"MapObjects\window_closed";
        public const string OBJ_WINDOW_OPEN = @"MapObjects\window_open";
        public const string OBJ_WINDOW_BROKEN = @"MapObjects\window_broken";

        public const string OBJ_BENCH = @"MapObjects\bench";
        public const string OBJ_FENCE = @"MapObjects\fence";

        public const string OBJ_CAR1 = @"MapObjects\car1";
        public const string OBJ_CAR2 = @"MapObjects\car2";
        public const string OBJ_CAR3 = @"MapObjects\car3";
        public const string OBJ_CAR4 = @"MapObjects\car4";

        public const string OBJ_SHOP_SHELF = @"MapObjects\shop_shelf";
        public const string OBJ_BED = @"MapObjects\bed";
        public const string OBJ_WARDROBE = @"MapObjects\wardrobe";
        public const string OBJ_TABLE = @"MapObjects\table";
        public const string OBJ_FRIDGE = @"MapObjects\fridge";
        public const string OBJ_DRAWER = @"MapObjects\drawer";
        public const string OBJ_CHAIR = @"MapObjects\chair";
        public const string OBJ_NIGHT_TABLE = @"MapObjects\nighttable";
        public const string OBJ_CHAR_CHAIR = @"MapObjects\char_chair";
        public const string OBJ_CHAR_TABLE = @"MapObjects\char_table";

        public const string OBJ_IRON_BENCH = @"MapObjects\iron_bench";
        public const string OBJ_IRON_DOOR_OPEN = @"MapObjects\iron_door_open";
        public const string OBJ_IRON_DOOR_CLOSED = @"MapObjects\iron_door_closed";
        public const string OBJ_IRON_DOOR_BROKEN = @"MapObjects\iron_door_broken";
        public const string OBJ_IRON_FENCE = @"MapObjects\iron_fence";

        public const string OBJ_BARRELS = @"MapObjects\barrels";
        public const string OBJ_JUNK = @"MapObjects\junk";

        public const string OBJ_POWERGEN_OFF = @"MapObjects\power_generator_off";
        public const string OBJ_POWERGEN_ON = @"MapObjects\power_generator_on";

        public const string OBJ_GATE_CLOSED = @"MapObjects\gate_closed";
        public const string OBJ_GATE_OPEN = @"MapObjects\gate_open";

        public const string OBJ_BOARD = @"MapObjects\announcement_board";

        public const string OBJ_SMALL_WOODEN_FORTIFICATION = @"MapObjects\wooden_small_fortification";
        public const string OBJ_LARGE_WOODEN_FORTIFICATION = @"MapObjects\wooden_large_fortification";

        public const string OBJ_HOSPITAL_BED = @"MapObjects\hospital_bed";
        public const string OBJ_HOSPITAL_CHAIR = @"MapObjects\hospital_chair";
        public const string OBJ_HOSPITAL_NIGHT_TABLE = @"MapObjects\hospital_nighttable";
        public const string OBJ_HOSPITAL_WARDROBE = @"MapObjects\hospital_wardrobe";
        public const string OBJ_HOSPITAL_DOOR_OPEN = @"MapObjects\hospital_door_open";
        public const string OBJ_HOSPITAL_DOOR_CLOSED = @"MapObjects\hospital_door_closed";
        public const string OBJ_HOSPITAL_DOOR_BROKEN = @"MapObjects\hospital_door_broken";

        public const string OBJ_GARDEN_FENCE = @"MapObjects\garden_fence";
        public const string OBJ_WIRE_FENCE = @"MapObjects\wire_fence";

        public const string PLAYER_FOLLOWER = @"Actors\player_follower";
        public const string PLAYER_FOLLOWER_TRUST = @"Actors\player_follower_trust";
        public const string PLAYER_FOLLOWER_BOND = @"Actors\player_follower_bond";

        public const string ACTOR_SKELETON = @"Actors\skeleton";
        public const string ACTOR_RED_EYED_SKELETON = @"Actors\red_eyed_skeleton";
        public const string ACTOR_RED_SKELETON = @"Actors\red_skeleton";
        public const string ACTOR_ZOMBIE = @"Actors\zombie";
        public const string ACTOR_DARK_EYED_ZOMBIE = @"Actors\dark_eyed_zombie";
        public const string ACTOR_DARK_ZOMBIE = @"Actors\dark_zombie";
        public const string ACTOR_MALE_NEOPHYTE = @"Actors\male_neophyte";
        public const string ACTOR_FEMALE_NEOPHYTE = @"Actors\female_neophyte";
        public const string ACTOR_MALE_DISCIPLE = @"Actors\male_disciple";
        public const string ACTOR_FEMALE_DISCIPLE = @"Actors\female_disciple";
        public const string ACTOR_ZOMBIE_MASTER = @"Actors\zombie_master";
        public const string ACTOR_ZOMBIE_LORD = @"Actors\zombie_lord";
        public const string ACTOR_ZOMBIE_PRINCE = @"Actors\zombie_prince";
        public const string ACTOR_RAT_ZOMBIE = @"Actors\rat_zombie";
        public const string ACTOR_SEWERS_THING = @"Actors\sewers_thing";
        public const string ACTOR_JASON_MYERS = @"Actors\jason_myers";
        public const string ACTOR_BIG_BEAR = @"Actors\big_bear";
        public const string ACTOR_FAMU_FATARU = @"Actors\famu_fataru";
        public const string ACTOR_SANTAMAN = @"Actors\santaman";
        public const string ACTOR_ROGUEDJACK = @"Actors\roguedjack";
        public const string ACTOR_DUCKMAN = @"Actors\duckman";
        public const string ACTOR_HANS_VON_HANZ = @"Actors\hans_von_hanz";

        public const string BLOODIED = @"Actors\Decoration\bloodied";

        public const string MALE_SKIN1 = @"Actors\Decoration\male_skin1";
        public const string MALE_SKIN2 = @"Actors\Decoration\male_skin2";
        public const string MALE_SKIN3 = @"Actors\Decoration\male_skin3";
        public const string MALE_SKIN4 = @"Actors\Decoration\male_skin4";
        public const string MALE_SKIN5 = @"Actors\Decoration\male_skin5";
        public const string MALE_HAIR1 = @"Actors\Decoration\male_hair1";
        public const string MALE_HAIR2 = @"Actors\Decoration\male_hair2";
        public const string MALE_HAIR3 = @"Actors\Decoration\male_hair3";
        public const string MALE_HAIR4 = @"Actors\Decoration\male_hair4";
        public const string MALE_HAIR5 = @"Actors\Decoration\male_hair5";
        public const string MALE_HAIR6 = @"Actors\Decoration\male_hair6";
        public const string MALE_HAIR7 = @"Actors\Decoration\male_hair7";
        public const string MALE_HAIR8 = @"Actors\Decoration\male_hair8";
        public const string MALE_SHIRT1 = @"Actors\Decoration\male_shirt1";
        public const string MALE_SHIRT2 = @"Actors\Decoration\male_shirt2";
        public const string MALE_SHIRT3 = @"Actors\Decoration\male_shirt3";
        public const string MALE_SHIRT4 = @"Actors\Decoration\male_shirt4";
        public const string MALE_SHIRT5 = @"Actors\Decoration\male_shirt5";
        public const string MALE_PANTS1 = @"Actors\Decoration\male_pants1";
        public const string MALE_PANTS2 = @"Actors\Decoration\male_pants2";
        public const string MALE_PANTS3 = @"Actors\Decoration\male_pants3";
        public const string MALE_PANTS4 = @"Actors\Decoration\male_pants4";
        public const string MALE_PANTS5 = @"Actors\Decoration\male_pants5";
        public const string MALE_SHOES1 = @"Actors\Decoration\male_shoes1";
        public const string MALE_SHOES2 = @"Actors\Decoration\male_shoes2";
        public const string MALE_SHOES3 = @"Actors\Decoration\male_shoes3";
        public const string MALE_EYES1 = @"Actors\Decoration\male_eyes1";
        public const string MALE_EYES2 = @"Actors\Decoration\male_eyes2";
        public const string MALE_EYES3 = @"Actors\Decoration\male_eyes3";
        public const string MALE_EYES4 = @"Actors\Decoration\male_eyes4";
        public const string MALE_EYES5 = @"Actors\Decoration\male_eyes5";
        public const string MALE_EYES6 = @"Actors\Decoration\male_eyes6";

        public const string FEMALE_SKIN1 = @"Actors\Decoration\female_skin1";
        public const string FEMALE_SKIN2 = @"Actors\Decoration\female_skin2";
        public const string FEMALE_SKIN3 = @"Actors\Decoration\female_skin3";
        public const string FEMALE_SKIN4 = @"Actors\Decoration\female_skin4";
        public const string FEMALE_SKIN5 = @"Actors\Decoration\female_skin5";
        public const string FEMALE_HAIR1 = @"Actors\Decoration\female_hair1";
        public const string FEMALE_HAIR2 = @"Actors\Decoration\female_hair2";
        public const string FEMALE_HAIR3 = @"Actors\Decoration\female_hair3";
        public const string FEMALE_HAIR4 = @"Actors\Decoration\female_hair4";
        public const string FEMALE_HAIR5 = @"Actors\Decoration\female_hair5";
        public const string FEMALE_HAIR6 = @"Actors\Decoration\female_hair6";
        public const string FEMALE_HAIR7 = @"Actors\Decoration\female_hair7";
        public const string FEMALE_SHIRT1 = @"Actors\Decoration\female_shirt1";
        public const string FEMALE_SHIRT2 = @"Actors\Decoration\female_shirt2";
        public const string FEMALE_SHIRT3 = @"Actors\Decoration\female_shirt3";
        public const string FEMALE_SHIRT4 = @"Actors\Decoration\female_shirt4";
        public const string FEMALE_PANTS1 = @"Actors\Decoration\female_pants1";
        public const string FEMALE_PANTS2 = @"Actors\Decoration\female_pants2";
        public const string FEMALE_PANTS3 = @"Actors\Decoration\female_pants3";
        public const string FEMALE_PANTS4 = @"Actors\Decoration\female_pants4";
        public const string FEMALE_PANTS5 = @"Actors\Decoration\female_pants5";
        public const string FEMALE_SHOES1 = @"Actors\Decoration\female_shoes1";
        public const string FEMALE_SHOES2 = @"Actors\Decoration\female_shoes2";
        public const string FEMALE_SHOES3 = @"Actors\Decoration\female_shoes3";
        public const string FEMALE_EYES1 = @"Actors\Decoration\female_eyes1";
        public const string FEMALE_EYES2 = @"Actors\Decoration\female_eyes2";
        public const string FEMALE_EYES3 = @"Actors\Decoration\female_eyes3";
        public const string FEMALE_EYES4 = @"Actors\Decoration\female_eyes4";
        public const string FEMALE_EYES5 = @"Actors\Decoration\female_eyes5";
        public const string FEMALE_EYES6 = @"Actors\Decoration\female_eyes6";

        public const string ARMY_HELMET = @"Actors\Decoration\army_helmet";
        public const string ARMY_PANTS = @"Actors\Decoration\army_pants";
        public const string ARMY_SHIRT = @"Actors\Decoration\army_shirt";
        public const string ARMY_SHOES = @"Actors\Decoration\army_shoes";

        public const string BIKER_HAIR1 = @"Actors\Decoration\biker_hair1";
        public const string BIKER_HAIR2 = @"Actors\Decoration\biker_hair2";
        public const string BIKER_HAIR3 = @"Actors\Decoration\biker_hair3";
        public const string BIKER_PANTS = @"Actors\Decoration\biker_pants";
        public const string BIKER_SHOES = @"Actors\Decoration\biker_shoes";

        public const string GANGSTA_HAT = @"Actors\Decoration\gangsta_hat";
        public const string GANGSTA_PANTS = @"Actors\Decoration\gangsta_pants";
        public const string GANGSTA_SHIRT = @"Actors\Decoration\gangsta_shirt";

        public const string CHARGUARD_HAIR = @"Actors\Decoration\charguard_hair";
        public const string CHARGUARD_PANTS = @"Actors\Decoration\charguard_pants";

        public const string POLICE_HAT = @"Actors\Decoration\police_hat";
        public const string POLICE_UNIFORM = @"Actors\Decoration\police_uniform";
        public const string POLICE_PANTS = @"Actors\Decoration\police_pants";
        public const string POLICE_SHOES = @"Actors\Decoration\police_shoes";

        public const string BLACKOP_SUIT = @"Actors\Decoration\blackop_suit";

        public const string HOSPITAL_DOCTOR_UNIFORM = @"Actors\Decoration\hospital_doctor_uniform";
        public const string HOSPITAL_NURSE_UNIFORM = @"Actors\Decoration\hospital_nurse_uniform";
        public const string HOSPITAL_PATIENT_UNIFORM = @"Actors\Decoration\hospital_patient_uniform";

        public const string SURVIVOR_MALE_BANDANA = @"Actors\Decoration\survivor_male_bandana";
        public const string SURVIVOR_FEMALE_BANDANA = @"Actors\Decoration\survivor_female_bandana";

        public const string DOG_SKIN1 = @"Actors\Decoration\dog_skin1";
        public const string DOG_SKIN2 = @"Actors\Decoration\dog_skin2";
        public const string DOG_SKIN3 = @"Actors\Decoration\dog_skin3";

        public const string ITEM_SLOT = @"Items\itemslot";
        public const string ITEM_EQUIPPED = @"Items\itemequipped";

        public const string ITEM_AMMO_LIGHT_PISTOL = @"Items\item_ammo_light_pistol";
        public const string ITEM_AMMO_HEAVY_PISTOL = @"Items\item_ammo_heavy_pistol";
        public const string ITEM_AMMO_LIGHT_RIFLE = @"Items\item_ammo_light_rifle";
        public const string ITEM_AMMO_HEAVY_RIFLE = @"Items\item_ammo_heavy_rifle";
        public const string ITEM_AMMO_SHOTGUN = @"Items\item_ammo_shotgun";
        public const string ITEM_AMMO_BOLTS = @"Items\item_ammo_bolts";

        public const string ITEM_ARMY_BODYARMOR = @"Items\item_army_bodyarmor";
        public const string ITEM_ARMY_PISTOL = @"Items\item_army_pistol";
        public const string ITEM_ARMY_RATION = @"Items\item_army_ration";
        public const string ITEM_ARMY_RIFLE = @"Items\item_army_rifle";
        public const string ITEM_BANDAGES = @"Items\item_bandages";
        public const string ITEM_BARBED_WIRE = @"Items\item_barbed_wire";
        public const string ITEM_BEAR_TRAP = @"Items\item_bear_trap";
        public const string ITEM_BASEBALL_BAT = @"Items\item_baseballbat";
        public const string ITEM_BIGBEAR_BAT = @"Items\item_bigbear_bat";
        public const string ITEM_BIG_FLASHLIGHT = @"Items\item_big_flashlight";
        public const string ITEM_BIG_FLASHLIGHT_OUT = @"Items\item_big_flashlight_out";
        public const string ITEM_BOOK = @"Items\item_book";
        public const string ITEM_BLACKOPS_GPS = @"Items\item_blackops_gps";
        public const string ITEM_CANNED_FOOD = @"Items\item_canned_food";
        public const string ITEM_CELL_PHONE = @"Items\item_cellphone";
        public const string ITEM_CHAR_LIGHT_BODYARMOR = @"Items\item_CHAR_light_bodyarmor";
        public const string ITEM_CROWBAR = @"Items\item_crowbar";
        public const string ITEM_COMBAT_KNIFE = @"Items\item_combat_knife";
        public const string ITEM_EMPTY_CAN = @"Items\item_empty_can";
        public const string ITEM_FAMU_FATARU_KATANA = @"Items\item_famu_fataru_katana";
        public const string ITEM_FLASHLIGHT = @"Items\item_flashlight";
        public const string ITEM_FLASHLIGHT_OUT = @"Items\item_flashlight_out";
        public const string ITEM_FREE_ANGELS_JACKET = @"Items\item_free_angels_jacket";
        public const string ITEM_GRENADE = @"Items\item_grenade";
        public const string ITEM_GRENADE_PRIMED = @"Items\item_grenade_primed";
        public const string ITEM_JASON_MYERS_AXE = @"Items\item_jason_myers_axe";
        public const string ITEM_GOLF_CLUB = @"Items\item_golfclub";
        public const string ITEM_GROCERIES = @"Items\item_groceries";
        public const string ITEM_HANS_VON_HANZ_PISTOL = @"Items\item_hans_von_hanz_pistol";
        public const string ITEM_HELLS_SOULS_JACKET = @"Items\item_hells_souls_jacket";
        public const string ITEM_HUGE_HAMMER = @"Items\item_huge_hammer";
        public const string ITEM_HUNTER_VEST = @"Items\item_hunter_vest";
        public const string ITEM_HUNTING_CROSSBOW = @"Items\item_hunting_crossbow";
        public const string ITEM_HUNTING_RIFLE = @"Items\item_hunting_rifle";
        public const string ITEM_IMPROVISED_CLUB = @"Items\item_improvised_club";
        public const string ITEM_IMPROVISED_SPEAR = @"Items\item_improvised_spear";
        public const string ITEM_IRON_GOLF_CLUB = @"Items\item_iron_golfclub";
        public const string ITEM_KOLT_REVOLVER = @"Items\item_kolt_revolver";
        public const string ITEM_MAGAZINE = @"Items\item_magazine";
        public const string ITEM_MEDIKIT = @"Items\item_medikit";
        public const string ITEM_PISTOL = @"Items\item_pistol";
        public const string ITEM_PILLS_ANTIVIRAL = @"Items\item_pills_antiviral";
        public const string ITEM_PILLS_BLUE = @"Items\item_pills_blue";
        public const string ITEM_PILLS_GREEN = @"Items\item_pills_green";
        public const string ITEM_PILLS_SAN = @"Items\item_pills_san";
        public const string ITEM_POLICE_JACKET = @"Items\item_police_jacket";
        public const string ITEM_POLICE_RADIO = @"Items\item_police_radio";
        public const string ITEM_POLICE_RIOT_ARMOR = @"Items\item_police_riot_armor";
        public const string ITEM_PRECISION_RIFLE = @"Items\item_precision_rifle";
        public const string ITEM_ROGUEDJACK_KEYBOARD = @"Items\item_roguedjack_keyboard";
        public const string ITEM_SANTAMAN_SHOTGUN = @"Items\item_santaman_shotgun";
        public const string ITEM_SHOTGUN = @"Items\item_shotgun";
        public const string ITEM_SHOVEL = @"Items\item_shovel";
        public const string ITEM_SMALL_HAMMER = @"Items\item_small_hammer";
        public const string ITEM_SPIKES = @"Items\item_spikes";
        public const string ITEM_SHORT_SHOVEL = @"Items\item_short_shovel";
        public const string ITEM_SPRAYPAINT = @"Items\item_spraypaint";
        public const string ITEM_SPRAYPAINT2 = @"Items\item_spraypaint2";
        public const string ITEM_SPRAYPAINT3 = @"Items\item_spraypaint3";
        public const string ITEM_SPRAYPAINT4 = @"Items\item_spraypaint4";
        public const string ITEM_STENCH_KILLER = @"Items\item_stench_killer";
        public const string ITEM_SUBWAY_BADGE = @"Items\item_subway_badge";
        public const string ITEM_TRUNCHEON = @"Items\item_truncheon";
        public const string ITEM_WOODEN_PLANK = @"Items\item_wooden_plank";
        public const string ITEM_ZTRACKER = @"Items\item_ztracker";

        public const string EFFECT_BARRICADED = @"Effects\barricaded";
        public const string EFFECT_ONFIRE = @"Effects\onFire";

        public const string UNDEF = @"undef";
        public const string MAP_EXIT = @"map_exit";
        public const string MINI_PLAYER_POSITION = @"mini_player_position";
        public const string MINI_PLAYER_TAG1 = @"mini_player_tag";
        public const string MINI_PLAYER_TAG2 = @"mini_player_tag2";
        public const string MINI_PLAYER_TAG3 = @"mini_player_tag3";
        public const string MINI_PLAYER_TAG4 = @"mini_player_tag4";
        public const string MINI_FOLLOWER_POSITION = @"mini_follower_position";
        public const string MINI_UNDEAD_POSITION = @"mini_undead_position";
        public const string MINI_BLACKOPS_POSITION = @"mini_blackops_position";
        public const string MINI_POLICE_POSITION = @"mini_police_position";
        public const string TRACK_FOLLOWER_POSITION = @"track_follower_position";
        public const string TRACK_UNDEAD_POSITION = @"track_undead_position";
        public const string TRACK_BLACKOPS_POSITION = @"track_blackops_position";
        public const string TRACK_POLICE_POSITION = @"track_police_position";

        public const string WEATHER_RAIN1 = @"weather_rain1";
        public const string WEATHER_RAIN2 = @"weather_rain2";
        public const string WEATHER_HEAVY_RAIN1 = @"weather_heavy_rain1";
        public const string WEATHER_HEAVY_RAIN2 = @"weather_heavy_rain2";

        public const string CORPSE_DRAGGED = @"corpse_dragged";
        public const string ROT1_1 = @"rot1_1";
        public const string ROT1_2 = @"rot1_2";
        public const string ROT2_1 = @"rot2_1";
        public const string ROT2_2 = @"rot2_2";
        public const string ROT3_1 = @"rot3_1";
        public const string ROT3_2 = @"rot3_2";
        public const string ROT4_1 = @"rot4_1";
        public const string ROT4_2 = @"rot4_2";
        public const string ROT5_1 = @"rot5_1";
        public const string ROT5_2 = @"rot5_2";

        private static GraphicsDevice graphicsDevice;
        private static TextureLoader textureLoader;

        static readonly Dictionary<string, Texture2D> s_Images = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Texture2D> s_GrayLevelImages = new Dictionary<string, Texture2D>();

        public static void LoadResources(GameLoader loader, GraphicsDevice _graphicsDevice)
        {
            graphicsDevice = _graphicsDevice;
            textureLoader = new TextureLoader(graphicsDevice);

            loader.CategoryStart("Loading images...");

            loader.LoadImage(ACTIVITY_CHASING);
            loader.LoadImage(ACTIVITY_CHASING_PLAYER);
            loader.LoadImage(ACTIVITY_TRACKING);
            loader.LoadImage(ACTIVITY_FLEEING);
            loader.LoadImage(ACTIVITY_FLEEING_FROM_EXPLOSIVE);
            loader.LoadImage(ACTIVITY_FOLLOWING);
            loader.LoadImage(ACTIVITY_FOLLOWING_ORDER);
            loader.LoadImage(ACTIVITY_FOLLOWING_PLAYER);
            loader.LoadImage(ACTIVITY_FOLLOWING_LEADER);
            loader.LoadImage(ACTIVITY_SLEEPING);

            loader.LoadImage(ICON_EXPIRED_FOOD);
            loader.LoadImage(ICON_TARGET);
            loader.LoadImage(ICON_MELEE_ATTACK);
            loader.LoadImage(ICON_MELEE_MISS);
            loader.LoadImage(ICON_MELEE_DAMAGE);
            loader.LoadImage(ICON_RANGED_ATTACK);
            loader.LoadImage(ICON_RANGED_DAMAGE);
            loader.LoadImage(ICON_RANGED_MISS);
            loader.LoadImage(ICON_KILLED);
            loader.LoadImage(ICON_LEADER);
            loader.LoadImage(ICON_RUNNING);
            loader.LoadImage(ICON_CANT_RUN);
            loader.LoadImage(ICON_CAN_TRADE);
            loader.LoadImage(ICON_HAS_VITAL_ITEM);
            loader.LoadImage(ICON_OUT_OF_AMMO);
            loader.LoadImage(ICON_OUT_OF_BATTERIES);
            loader.LoadImage(ICON_SLEEP_ALMOST_SLEEPY);
            loader.LoadImage(ICON_SLEEP_SLEEPY);
            loader.LoadImage(ICON_SLEEP_EXHAUSTED);
            loader.LoadImage(ICON_SPOILED_FOOD);
            loader.LoadImage(ICON_FOOD_ALMOST_HUNGRY);
            loader.LoadImage(ICON_FOOD_HUNGRY);
            loader.LoadImage(ICON_FOOD_STARVING);
            loader.LoadImage(ICON_LINE_BAD);
            loader.LoadImage(ICON_LINE_BLOCKED);
            loader.LoadImage(ICON_LINE_CLEAR);
            loader.LoadImage(ICON_BLAST);
            loader.LoadImage(ICON_HEALING);
            loader.LoadImage(ICON_IS_TARGET);
            loader.LoadImage(ICON_IS_TARGETTED);
            loader.LoadImage(ICON_IS_TARGETING);
            loader.LoadImage(ICON_IS_IN_GROUP);
            loader.LoadImage(ICON_THREAT_DANGER);
            loader.LoadImage(ICON_THREAT_HIGH_DANGER);
            loader.LoadImage(ICON_THREAT_SAFE);
            loader.LoadImage(ICON_SCENT_LIVING);
            loader.LoadImage(ICON_SCENT_ZOMBIEMASTER);
            loader.LoadImage(ICON_ODOR_SUPPRESSED);
            loader.LoadImage(ICON_SELF_DEFENCE);
            loader.LoadImage(ICON_INDIRECT_ENEMIES);
            loader.LoadImage(ICON_AGGRESSOR);
            loader.LoadImage(ICON_TRAP_ACTIVATED);
            loader.LoadImage(ICON_TRAP_ACTIVATED_SAFE_GROUP);
            loader.LoadImage(ICON_TRAP_ACTIVATED_SAFE_PLAYER);
            loader.LoadImage(ICON_TRAP_TRIGGERED);
            loader.LoadImage(ICON_TRAP_TRIGGERED_SAFE_GROUP);
            loader.LoadImage(ICON_TRAP_TRIGGERED_SAFE_PLAYER);
            loader.LoadImage(ICON_ROT_ALMOST_HUNGRY);
            loader.LoadImage(ICON_ROT_HUNGRY);
            loader.LoadImage(ICON_ROT_STARVING);
            loader.LoadImage(ICON_SANITY_INSANE);
            loader.LoadImage(ICON_SANITY_DISTURBED);
            loader.LoadImage(ICON_BORING_ITEM);
            loader.LoadImage(ICON_ZGRAB);

            loader.LoadImage(TILE_FLOOR_ASPHALT);
            loader.LoadImage(TILE_FLOOR_CONCRETE);
            loader.LoadImage(TILE_FLOOR_GRASS);
            loader.LoadImage(TILE_FLOOR_OFFICE);
            loader.LoadImage(TILE_FLOOR_PLANKS);
            loader.LoadImage(TILE_FLOOR_SEWER_WATER);
            loader.LoadImage(TILE_FLOOR_SEWER_WATER_ANIM1);
            loader.LoadImage(TILE_FLOOR_SEWER_WATER_ANIM2);
            loader.LoadImage(TILE_FLOOR_SEWER_WATER_ANIM3);
            loader.LoadImage(TILE_FLOOR_SEWER_WATER_COVER);
            loader.LoadImage(TILE_FLOOR_TILES);
            loader.LoadImage(TILE_FLOOR_WALKWAY);

            loader.LoadImage(TILE_ROAD_ASPHALT_NS);
            loader.LoadImage(TILE_ROAD_ASPHALT_EW);
            loader.LoadImage(TILE_RAIL_ES);

            loader.LoadImage(TILE_WALL_BRICK);
            loader.LoadImage(TILE_WALL_CHAR_OFFICE);
            loader.LoadImage(TILE_WALL_HOSPITAL);
            loader.LoadImage(TILE_WALL_SEWER);
            loader.LoadImage(TILE_WALL_STONE);

            loader.LoadImage(DECO_BLOODIED_FLOOR);
            loader.LoadImage(DECO_BLOODIED_WALL);
            loader.LoadImage(DECO_ZOMBIE_REMAINS);
            loader.LoadImage(DECO_VOMIT);

            loader.LoadImage(DECO_POSTERS1);
            loader.LoadImage(DECO_POSTERS2);
            loader.LoadImage(DECO_TAGS1);
            loader.LoadImage(DECO_TAGS2);
            loader.LoadImage(DECO_TAGS3);
            loader.LoadImage(DECO_TAGS4);
            loader.LoadImage(DECO_TAGS5);
            loader.LoadImage(DECO_TAGS6);
            loader.LoadImage(DECO_TAGS7);

            loader.LoadImage(DECO_SHOP_CONSTRUCTION);
            loader.LoadImage(DECO_SHOP_GENERAL_STORE);
            loader.LoadImage(DECO_SHOP_GROCERY);
            loader.LoadImage(DECO_SHOP_GUNSHOP);
            loader.LoadImage(DECO_SHOP_PHARMACY);
            loader.LoadImage(DECO_SHOP_SPORTSWEAR);
            loader.LoadImage(DECO_SHOP_HUNTING);

            loader.LoadImage(DECO_CHAR_OFFICE);
            loader.LoadImage(DECO_CHAR_FLOOR_LOGO);
            loader.LoadImage(DECO_CHAR_POSTER1);
            loader.LoadImage(DECO_CHAR_POSTER2);
            loader.LoadImage(DECO_CHAR_POSTER3);

            loader.LoadImage(DECO_PLAYER_TAG1);
            loader.LoadImage(DECO_PLAYER_TAG2);
            loader.LoadImage(DECO_PLAYER_TAG3);
            loader.LoadImage(DECO_PLAYER_TAG4);

            loader.LoadImage(DECO_ROGUEDJACK_TAG);

            loader.LoadImage(DECO_SEWER_LADDER);
            loader.LoadImage(DECO_SEWER_HOLE);
            loader.LoadImage(DECO_SEWERS_BUILDING);

            loader.LoadImage(DECO_SUBWAY_BUILDING);

            loader.LoadImage(DECO_STAIRS_DOWN);
            loader.LoadImage(DECO_STAIRS_UP);

            loader.LoadImage(DECO_POWER_SIGN_BIG);

            loader.LoadImage(DECO_POLICE_STATION);

            loader.LoadImage(DECO_HOSPITAL);

            loader.LoadImage(OBJ_TREE);

            loader.LoadImage(OBJ_WOODEN_DOOR_CLOSED);
            loader.LoadImage(OBJ_WOODEN_DOOR_OPEN);
            loader.LoadImage(OBJ_WOODEN_DOOR_BROKEN);

            loader.LoadImage(OBJ_GLASS_DOOR_CLOSED);
            loader.LoadImage(OBJ_GLASS_DOOR_OPEN);
            loader.LoadImage(OBJ_GLASS_DOOR_BROKEN);

            loader.LoadImage(OBJ_CHAR_DOOR_BROKEN);
            loader.LoadImage(OBJ_CHAR_DOOR_CLOSED);
            loader.LoadImage(OBJ_CHAR_DOOR_OPEN);

            loader.LoadImage(OBJ_WINDOW_CLOSED);
            loader.LoadImage(OBJ_WINDOW_OPEN);
            loader.LoadImage(OBJ_WINDOW_BROKEN);

            loader.LoadImage(OBJ_BENCH);
            loader.LoadImage(OBJ_FENCE);

            loader.LoadImage(OBJ_CAR1);
            loader.LoadImage(OBJ_CAR2);
            loader.LoadImage(OBJ_CAR3);
            loader.LoadImage(OBJ_CAR4);

            loader.LoadImage(OBJ_SHOP_SHELF);
            loader.LoadImage(OBJ_BED);
            loader.LoadImage(OBJ_WARDROBE);
            loader.LoadImage(OBJ_TABLE);
            loader.LoadImage(OBJ_FRIDGE);
            loader.LoadImage(OBJ_DRAWER);
            loader.LoadImage(OBJ_CHAIR);
            loader.LoadImage(OBJ_NIGHT_TABLE);

            loader.LoadImage(OBJ_CHAR_CHAIR);
            loader.LoadImage(OBJ_CHAR_TABLE);

            loader.LoadImage(OBJ_IRON_BENCH);
            loader.LoadImage(OBJ_IRON_FENCE);
            loader.LoadImage(OBJ_IRON_DOOR_BROKEN);
            loader.LoadImage(OBJ_IRON_DOOR_CLOSED);
            loader.LoadImage(OBJ_IRON_DOOR_OPEN);

            loader.LoadImage(OBJ_BARRELS);
            loader.LoadImage(OBJ_JUNK);

            loader.LoadImage(OBJ_POWERGEN_OFF);
            loader.LoadImage(OBJ_POWERGEN_ON);

            loader.LoadImage(OBJ_GATE_CLOSED);
            loader.LoadImage(OBJ_GATE_OPEN);

            loader.LoadImage(OBJ_BOARD);

            loader.LoadImage(OBJ_SMALL_WOODEN_FORTIFICATION);
            loader.LoadImage(OBJ_LARGE_WOODEN_FORTIFICATION);

            loader.LoadImage(OBJ_HOSPITAL_BED);
            loader.LoadImage(OBJ_HOSPITAL_CHAIR);
            loader.LoadImage(OBJ_HOSPITAL_DOOR_BROKEN);
            loader.LoadImage(OBJ_HOSPITAL_DOOR_CLOSED);
            loader.LoadImage(OBJ_HOSPITAL_DOOR_OPEN);
            loader.LoadImage(OBJ_HOSPITAL_NIGHT_TABLE);
            loader.LoadImage(OBJ_HOSPITAL_WARDROBE);

            loader.LoadImage(OBJ_GARDEN_FENCE);
            loader.LoadImage(OBJ_WIRE_FENCE);

            loader.LoadImage(PLAYER_FOLLOWER);
            loader.LoadImage(PLAYER_FOLLOWER_TRUST);
            loader.LoadImage(PLAYER_FOLLOWER_BOND);

            loader.LoadImage(ACTOR_SKELETON);
            loader.LoadImage(ACTOR_RED_EYED_SKELETON);
            loader.LoadImage(ACTOR_RED_SKELETON);
            loader.LoadImage(ACTOR_ZOMBIE);
            loader.LoadImage(ACTOR_DARK_EYED_ZOMBIE);
            loader.LoadImage(ACTOR_DARK_ZOMBIE);
            loader.LoadImage(ACTOR_MALE_NEOPHYTE);
            loader.LoadImage(ACTOR_FEMALE_NEOPHYTE);
            loader.LoadImage(ACTOR_MALE_DISCIPLE);
            loader.LoadImage(ACTOR_FEMALE_DISCIPLE);
            loader.LoadImage(ACTOR_ZOMBIE_MASTER);
            loader.LoadImage(ACTOR_ZOMBIE_LORD);
            loader.LoadImage(ACTOR_ZOMBIE_PRINCE);
            loader.LoadImage(ACTOR_RAT_ZOMBIE);
            loader.LoadImage(ACTOR_SEWERS_THING);
            loader.LoadImage(ACTOR_JASON_MYERS);
            loader.LoadImage(ACTOR_BIG_BEAR);
            loader.LoadImage(ACTOR_FAMU_FATARU);
            loader.LoadImage(ACTOR_SANTAMAN);
            loader.LoadImage(ACTOR_ROGUEDJACK);
            loader.LoadImage(ACTOR_DUCKMAN);
            loader.LoadImage(ACTOR_HANS_VON_HANZ);

            loader.LoadImage(BLOODIED);

            loader.LoadImage(MALE_SKIN1);
            loader.LoadImage(MALE_SKIN2);
            loader.LoadImage(MALE_SKIN3);
            loader.LoadImage(MALE_SKIN4);
            loader.LoadImage(MALE_SKIN5);
            loader.LoadImage(MALE_SHIRT1);
            loader.LoadImage(MALE_SHIRT2);
            loader.LoadImage(MALE_SHIRT3);
            loader.LoadImage(MALE_SHIRT4);
            loader.LoadImage(MALE_SHIRT5);
            loader.LoadImage(MALE_HAIR1);
            loader.LoadImage(MALE_HAIR2);
            loader.LoadImage(MALE_HAIR3);
            loader.LoadImage(MALE_HAIR4);
            loader.LoadImage(MALE_HAIR5);
            loader.LoadImage(MALE_HAIR6);
            loader.LoadImage(MALE_HAIR7);
            loader.LoadImage(MALE_HAIR8);
            loader.LoadImage(MALE_PANTS1);
            loader.LoadImage(MALE_PANTS2);
            loader.LoadImage(MALE_PANTS3);
            loader.LoadImage(MALE_PANTS4);
            loader.LoadImage(MALE_PANTS5);
            loader.LoadImage(MALE_SHOES1);
            loader.LoadImage(MALE_SHOES2);
            loader.LoadImage(MALE_SHOES3);
            loader.LoadImage(MALE_EYES1);
            loader.LoadImage(MALE_EYES2);
            loader.LoadImage(MALE_EYES3);
            loader.LoadImage(MALE_EYES4);
            loader.LoadImage(MALE_EYES5);
            loader.LoadImage(MALE_EYES6);

            loader.LoadImage(FEMALE_SKIN1);
            loader.LoadImage(FEMALE_SKIN2);
            loader.LoadImage(FEMALE_SKIN3);
            loader.LoadImage(FEMALE_SKIN4);
            loader.LoadImage(FEMALE_SKIN5);
            loader.LoadImage(FEMALE_SHIRT1);
            loader.LoadImage(FEMALE_SHIRT2);
            loader.LoadImage(FEMALE_SHIRT3);
            loader.LoadImage(FEMALE_SHIRT4);
            loader.LoadImage(FEMALE_HAIR1);
            loader.LoadImage(FEMALE_HAIR2);
            loader.LoadImage(FEMALE_HAIR3);
            loader.LoadImage(FEMALE_HAIR4);
            loader.LoadImage(FEMALE_HAIR5);
            loader.LoadImage(FEMALE_HAIR6);
            loader.LoadImage(FEMALE_HAIR7);
            loader.LoadImage(FEMALE_PANTS1);
            loader.LoadImage(FEMALE_PANTS2);
            loader.LoadImage(FEMALE_PANTS3);
            loader.LoadImage(FEMALE_PANTS4);
            loader.LoadImage(FEMALE_PANTS5);
            loader.LoadImage(FEMALE_SHOES1);
            loader.LoadImage(FEMALE_SHOES2);
            loader.LoadImage(FEMALE_SHOES3);
            loader.LoadImage(FEMALE_EYES1);
            loader.LoadImage(FEMALE_EYES2);
            loader.LoadImage(FEMALE_EYES3);
            loader.LoadImage(FEMALE_EYES4);
            loader.LoadImage(FEMALE_EYES5);
            loader.LoadImage(FEMALE_EYES6);

            loader.LoadImage(ARMY_HELMET);
            loader.LoadImage(ARMY_PANTS);
            loader.LoadImage(ARMY_SHIRT);
            loader.LoadImage(ARMY_SHOES);

            loader.LoadImage(BIKER_HAIR1);
            loader.LoadImage(BIKER_HAIR2);
            loader.LoadImage(BIKER_HAIR3);
            loader.LoadImage(BIKER_PANTS);
            loader.LoadImage(BIKER_SHOES);

            loader.LoadImage(GANGSTA_HAT);
            loader.LoadImage(GANGSTA_PANTS);
            loader.LoadImage(GANGSTA_SHIRT);

            loader.LoadImage(CHARGUARD_HAIR);
            loader.LoadImage(CHARGUARD_PANTS);

            loader.LoadImage(POLICE_HAT);
            loader.LoadImage(POLICE_PANTS);
            loader.LoadImage(POLICE_SHOES);
            loader.LoadImage(POLICE_UNIFORM);

            loader.LoadImage(BLACKOP_SUIT);

            loader.LoadImage(HOSPITAL_DOCTOR_UNIFORM);
            loader.LoadImage(HOSPITAL_NURSE_UNIFORM);
            loader.LoadImage(HOSPITAL_PATIENT_UNIFORM);

            loader.LoadImage(SURVIVOR_FEMALE_BANDANA);
            loader.LoadImage(SURVIVOR_MALE_BANDANA);

            loader.LoadImage(DOG_SKIN1);
            loader.LoadImage(DOG_SKIN2);
            loader.LoadImage(DOG_SKIN3);

            loader.LoadImage(ITEM_SLOT);
            loader.LoadImage(ITEM_EQUIPPED);

            loader.LoadImage(ITEM_AMMO_BOLTS);
            loader.LoadImage(ITEM_AMMO_HEAVY_PISTOL);
            loader.LoadImage(ITEM_AMMO_HEAVY_RIFLE);
            loader.LoadImage(ITEM_AMMO_LIGHT_PISTOL);
            loader.LoadImage(ITEM_AMMO_LIGHT_RIFLE);
            loader.LoadImage(ITEM_AMMO_SHOTGUN);
            loader.LoadImage(ITEM_ARMY_BODYARMOR);
            loader.LoadImage(ITEM_ARMY_PISTOL);
            loader.LoadImage(ITEM_ARMY_RATION);
            loader.LoadImage(ITEM_ARMY_RIFLE);
            loader.LoadImage(ITEM_BANDAGES);
            loader.LoadImage(ITEM_BARBED_WIRE);
            loader.LoadImage(ITEM_BASEBALL_BAT);
            loader.LoadImage(ITEM_BEAR_TRAP);
            loader.LoadImage(ITEM_BIGBEAR_BAT);
            loader.LoadImage(ITEM_BIG_FLASHLIGHT);
            loader.LoadImage(ITEM_BIG_FLASHLIGHT_OUT);
            loader.LoadImage(ITEM_BLACKOPS_GPS);
            loader.LoadImage(ITEM_BOOK);
            loader.LoadImage(ITEM_CANNED_FOOD);
            loader.LoadImage(ITEM_CELL_PHONE);
            loader.LoadImage(ITEM_CHAR_LIGHT_BODYARMOR);
            loader.LoadImage(ITEM_COMBAT_KNIFE);
            loader.LoadImage(ITEM_CROWBAR);
            loader.LoadImage(ITEM_EMPTY_CAN);
            loader.LoadImage(ITEM_FAMU_FATARU_KATANA);
            loader.LoadImage(ITEM_FLASHLIGHT);
            loader.LoadImage(ITEM_FLASHLIGHT_OUT);
            loader.LoadImage(ITEM_FREE_ANGELS_JACKET);
            loader.LoadImage(ITEM_GOLF_CLUB);
            loader.LoadImage(ITEM_GRENADE);
            loader.LoadImage(ITEM_GRENADE_PRIMED);
            loader.LoadImage(ITEM_GROCERIES);
            loader.LoadImage(ITEM_HELLS_SOULS_JACKET);
            loader.LoadImage(ITEM_HUGE_HAMMER);
            loader.LoadImage(ITEM_HUNTER_VEST);
            loader.LoadImage(ITEM_HUNTING_CROSSBOW);
            loader.LoadImage(ITEM_HUNTING_RIFLE);
            loader.LoadImage(ITEM_IMPROVISED_CLUB);
            loader.LoadImage(ITEM_IMPROVISED_SPEAR);
            loader.LoadImage(ITEM_IRON_GOLF_CLUB);
            loader.LoadImage(ITEM_JASON_MYERS_AXE);
            loader.LoadImage(ITEM_HANS_VON_HANZ_PISTOL);
            loader.LoadImage(ITEM_KOLT_REVOLVER);
            loader.LoadImage(ITEM_MAGAZINE);
            loader.LoadImage(ITEM_MEDIKIT);
            loader.LoadImage(ITEM_PILLS_ANTIVIRAL);
            loader.LoadImage(ITEM_PILLS_BLUE);
            loader.LoadImage(ITEM_PILLS_GREEN);
            loader.LoadImage(ITEM_PILLS_SAN);
            loader.LoadImage(ITEM_PISTOL);
            loader.LoadImage(ITEM_POLICE_JACKET);
            loader.LoadImage(ITEM_POLICE_RADIO);
            loader.LoadImage(ITEM_POLICE_RIOT_ARMOR);
            loader.LoadImage(ITEM_PRECISION_RIFLE);
            loader.LoadImage(ITEM_ROGUEDJACK_KEYBOARD);
            loader.LoadImage(ITEM_SANTAMAN_SHOTGUN);
            loader.LoadImage(ITEM_SHOTGUN);
            loader.LoadImage(ITEM_SHOVEL);
            loader.LoadImage(ITEM_SMALL_HAMMER);
            loader.LoadImage(ITEM_SHORT_SHOVEL);
            loader.LoadImage(ITEM_SPIKES);
            loader.LoadImage(ITEM_SPRAYPAINT);
            loader.LoadImage(ITEM_SPRAYPAINT2);
            loader.LoadImage(ITEM_SPRAYPAINT3);
            loader.LoadImage(ITEM_SPRAYPAINT4);
            loader.LoadImage(ITEM_STENCH_KILLER);
            loader.LoadImage(ITEM_SUBWAY_BADGE);
            loader.LoadImage(ITEM_TRUNCHEON);
            loader.LoadImage(ITEM_WOODEN_PLANK);
            loader.LoadImage(ITEM_ZTRACKER);

            loader.LoadImage(EFFECT_BARRICADED);
            loader.LoadImage(EFFECT_ONFIRE);

            loader.LoadImage(UNDEF);
            loader.LoadImage(MAP_EXIT);
            loader.LoadImage(MINI_BLACKOPS_POSITION);
            loader.LoadImage(MINI_FOLLOWER_POSITION);
            loader.LoadImage(MINI_PLAYER_POSITION);
            loader.LoadImage(MINI_PLAYER_TAG1);
            loader.LoadImage(MINI_PLAYER_TAG2);
            loader.LoadImage(MINI_PLAYER_TAG3);
            loader.LoadImage(MINI_PLAYER_TAG4);
            loader.LoadImage(MINI_POLICE_POSITION);
            loader.LoadImage(MINI_UNDEAD_POSITION);
            loader.LoadImage(TRACK_BLACKOPS_POSITION);
            loader.LoadImage(TRACK_FOLLOWER_POSITION);
            loader.LoadImage(TRACK_POLICE_POSITION);
            loader.LoadImage(TRACK_UNDEAD_POSITION);
            loader.LoadImage(WEATHER_RAIN1);
            loader.LoadImage(WEATHER_RAIN2);
            loader.LoadImage(WEATHER_HEAVY_RAIN1);
            loader.LoadImage(WEATHER_HEAVY_RAIN2);
            loader.LoadImage(ROT1_1);
            loader.LoadImage(ROT1_2);
            loader.LoadImage(ROT2_1);
            loader.LoadImage(ROT2_2);
            loader.LoadImage(ROT3_1);
            loader.LoadImage(ROT3_2);
            loader.LoadImage(ROT4_1);
            loader.LoadImage(ROT4_2);
            loader.LoadImage(ROT5_1);
            loader.LoadImage(ROT5_2);
            loader.LoadImage(CORPSE_DRAGGED);

            loader.CategoryEnd("Loading images done.");
        }

        public static void Load(string id)
        {
            string file = FOLDER + id + ".png";
            try
            {
                Texture2D img = textureLoader.FromFile(file);
                s_Images.Add(id, img);
                s_GrayLevelImages.Add(id, MakeGrayLevel(img));
            }
            catch (Exception)
            {
                throw new ArgumentException("coud not load image id=" + id + "; file=" + file);
            }
        }

        private static Texture2D MakeGrayLevel(Texture2D img)
        {
            Xna.Color[] inBufer = new Xna.Color[img.Width * img.Height];
            img.GetData(inBufer);

            Texture2D grayed = new Texture2D(graphicsDevice, img.Width, img.Height);
            Xna.Color[] outBufer = new Xna.Color[img.Width * img.Height];

            for (int x = 0; x < img.Width; x++)
                for (int y = 0; y < img.Height; y++)
                {
                    Xna.Color pixelColor = inBufer[x + y * img.Width];
                    float brightness = pixelColor.GetBrightness();
                    int rgb = (int)(255 * GRAYLEVEL_DIM_FACTOR * brightness);
                    outBufer[x + y * img.Width] = new Xna.Color(rgb, rgb, rgb, pixelColor.A);
                }

            grayed.SetData(outBufer);
            return grayed;
        }

        public static Texture2D Get(string imageID)
        {
            Texture2D img;
            if (s_Images.TryGetValue(imageID, out img))
                return img;
            else
                return s_Images[UNDEF];
        }

        public static Texture2D GetGrayLevel(string imageID)
        {
            Texture2D img;
            if (s_GrayLevelImages.TryGetValue(imageID, out img))
                return img;
            else
                return s_GrayLevelImages[UNDEF];
        }
    }
}

