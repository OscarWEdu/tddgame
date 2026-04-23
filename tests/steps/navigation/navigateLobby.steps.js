import { expect } from "@playwright/test";
import { createBdd } from 'playwright-bdd';

const { Given, When, Then } = createBdd();

Given("I am on the home page", async ({ page }) => {
  await page.goto("http://localhost:5173/");
});

When("I click the {string} button", async ({ page }, buttonName) => {
  await page.getByRole("button", { name: buttonName }).click();
});

Then("I should be taken to the lobby page", async ({ page }) => {
  await expect(page).toHaveURL(/.*\/lobby\/.*/);
});

export {};