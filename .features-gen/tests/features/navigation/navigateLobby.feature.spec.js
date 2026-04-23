// Generated from: tests\features\navigation\navigateLobby.feature
import { test } from "playwright-bdd";

test.describe('Navigate to lobby', () => {

  test('User navigates to lobby from home page', async ({ Given, When, Then, And, page }) => { 
    await Given('I am on the home page', null, { page }); 
    await When('I click the "Create Game" button', null, { page }); 
    await And('I click the "Create Game" button', null, { page }); 
    await And('I click the "Create Game" button', null, { page }); 
    await Then('I should be taken to the lobby page', null, { page }); 
  });

});

// == technical section ==

test.use({
  $test: [({}, use) => use(test), { scope: 'test', box: true }],
  $uri: [({}, use) => use('tests\\features\\navigation\\navigateLobby.feature'), { scope: 'test', box: true }],
  $bddFileData: [({}, use) => use(bddFileData), { scope: "test", box: true }],
});

const bddFileData = [ // bdd-data-start
  {"pwTestLine":6,"pickleLine":3,"tags":[],"steps":[{"pwStepLine":7,"gherkinStepLine":4,"keywordType":"Context","textWithKeyword":"Given I am on the home page","stepMatchArguments":[]},{"pwStepLine":8,"gherkinStepLine":5,"keywordType":"Action","textWithKeyword":"When I click the \"Create Game\" button","stepMatchArguments":[{"group":{"start":12,"value":"\"Create Game\"","children":[{"start":13,"value":"Create Game","children":[{"children":[]}]},{"children":[{"children":[]}]}]},"parameterTypeName":"string"}]},{"pwStepLine":9,"gherkinStepLine":6,"keywordType":"Action","textWithKeyword":"And I click the \"Create Game\" button","stepMatchArguments":[{"group":{"start":12,"value":"\"Create Game\"","children":[{"start":13,"value":"Create Game","children":[{"children":[]}]},{"children":[{"children":[]}]}]},"parameterTypeName":"string"}]},{"pwStepLine":10,"gherkinStepLine":7,"keywordType":"Action","textWithKeyword":"And I click the \"Create Game\" button","stepMatchArguments":[{"group":{"start":12,"value":"\"Create Game\"","children":[{"start":13,"value":"Create Game","children":[{"children":[]}]},{"children":[{"children":[]}]}]},"parameterTypeName":"string"}]},{"pwStepLine":11,"gherkinStepLine":8,"keywordType":"Outcome","textWithKeyword":"Then I should be taken to the lobby page","stepMatchArguments":[]}]},
]; // bdd-data-end