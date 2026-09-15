# Standardization Code Document

## Godot
- Version 4.4.1x (Most probably use 4.4.0) aka latest possible version

## Comments
- Should naturally adhere to concise comments under 80 characters. If the code itself is obvious we exclude comments altogether.
- We put comments above our code to give a brief overview. We should completely avoid putting comments to the side of the code.

## Naming conventions
- Each variable should concisely describe what it does. If it’s too hard to define, then it probably needs to be broken down into multiple variables, or switched into another type.
- Every file that has a class defined, should have the same name for the file itself and the class whenever possible.

## Folder pathing
- We use Pascal case for folders, files for naming convention (Example: res:/ModelThing/TextureMap/LOGO_1.png).
- External addons and other modules have lower-case convention (Example res:/modelThing/textureThing/idk_meme.png)
- Texture should have specification of the type, so a normal map might look like “GROUND_M”.
- SNAKE_CASE for our resources in ALL CAPS.
- We should always strive to break down our structures into more subsections. Overall everything should have a hierarchical structure to it. 

## Styling
- Brackets do not start on a new line
- Adequate spacing: Each semicolon, colon should be suffixed with a blank space
- Keywords such as static, const

## PascalCase for:
- Method names
- Enums, Namespaces and Interfaces
- Classes and Structs
- Public fields

## camelCase for
- Local variables.
- Global variables
- Parameters
- Private & protected fields. Internal additionally should have an underscore prefix (Example _variableThing)


## ALLCAPS with SNAKE_CASE if it’s multiple words
- Macros
- Static & Global variables

## Use of types
- We should avoid using ‘var’ as much as possible due to its implicitness. If we don’t know the type beforehand, we should perform checks and see if the type is a form that we do expect. For example:

if (Type is Bird bird) {
	// perform expected 
} else {
	// fallback function
}

## Forced encapsulation
- We should always strive to make all fields private unless we need them to be public
- Use of protected keywords whenever there is use of inheritance.

## Arguments
- We should parse references ideally, if a copy is required, then we make sure it goes out of scope, and isn’t further parsed to another function, which might be expecting a reference.
- If we want read-only access, we will parse it as a const naturally.

## Namespaces
- Should not go more than two levels deep
- Should only be used for specific cases and not in abundance

## Interfaces
- If we create tools, systems or other modules that are meant to be used by others, we should use an Interface to simplify the interaction between the two files.


# GitHub conventions and practices
- During every standup, we should specify beforehand which files we will be working on, and if we do decide we need to make changes in another file that someone else might be working on, then we need to signal it on Discord. This is to reduce the possibility of merge conflicts.
- Whenever we decide to push, we will always ‘pull’ from Github before pushing, as it’ll otherwise guarantee to be incompatible with the server repo. It can be good practice to do git fetch (Pulls but doesn’t automatically merge with your local repo) but that’s up to the individual. 
- We will only rely on separate branching whenever we are working in an isolated environment that can be developed without the need to interact with the rest of the system. One example for this might be the dogs because they are an independent scene, however the sled is a bad reason to branch for - because it would likely need to factor in the position of the dogs for its forward direction.
- Merging between two branches happens when another person has looked over a piece of code for proof-checking.

## When pushing to a branch, include in description
- Synopsis of change(s)
- What task the change correlates to
- Avoid large commits, more frequent and smaller commits are better
- Trunk-based, daily commits ideally. More frequent ones are better
