import { expect } from "@playwright/test";
import { createBdd } from 'playwright-bdd';

const { Given, When, Then } = createBdd();

Given("I am on the home page", async ({ page }) => {
  await page.goto("http://localhost:5173/");
});

When("I click the {string} button", async ({ page }, buttonName) => {
  await page.getByRole("button", { name: buttonName }).click();
});

When("I type {string} into the game name field", async ({ page }, text) => {
  await page.getByLabel("Game name").fill(text);
});

Then("I should be taken to the {string} page", async ({ page }, pageName) => {
  await expect(page).toHaveURL(new RegExp(`/${pageName}/`));
});

export {};