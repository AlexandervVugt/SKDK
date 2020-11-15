# Testing at different levels

![diagram](https://www.softwareautomationhelp.com/wp-content/uploads/2018/07/unittesting.jpg)

## Unit testing
[MSTest cheatsheet](https://www.automatetheplanet.com/mstest-cheat-sheet/)
### Wat is Unit Testing?
Unit testing is een vorm van software testing waarbij één specifiek onderdeel (Unit) in isolatie getest wordt. Over het algemeen houdt dit in dat je een class in isolatie - d.w.z. zonder invloed van buitenaf - test.
### Wanneer heb ik genoeg getest?
Een vuistregel hiervoor is dat elke public method getest moet zijn, en elk mogelijk path voor de method doorlopen moet worden. Daarnaast moet het gedrag van de method bij onwenselijke input getest worden. Heb je bijvoorbeeld een method die de inhoud van een kubus berekent, dan is een ribbe van -3 cm geen valide input. De method throwt dan een exception. In je unit test kun je d.m.v. een assertion aangeven dat je deze exception verwacht.
### Modified Condition-Decision Coverage
Modified Condition-Decision Coverage (MCDC) houdt grofweg het volgende in: Als je een method test, wil je dat alle conditions die een invloed hebben op de decision (return value) getest worden. Hierbij moet elke mogelijke waarde van een condition ten minste één keer bepalend zijn geweest voor de decision. Het idee achter MCDC is het elimineren van redundant test cases zodat je minder tests hoeft te schrijven. Op de onderstaande link kun je meer lezen over MCDC:
[MCDC wikipedia](https://nl.wikipedia.org/wiki/Modified_Condition_Decision_Coverage)

## Integration testing
### Mocking
#### Waarom Mocking?
Om de isolatie tijdens een Unit test te behouden, wil je dat het gedrag van andere Units voorspelbaar is, onafhankelijk van de implementatie. D.m.v. Mocking kun je de implementatie van een Unit negeren en deze precies laten doen wat je verwacht. Zo voorkom je dat jouw unit test een integration test wordt. Een praktisch voorbeeld:
```csharp
class A
{
  int x = 1;
  int calculateXY(B b)
  {
    return b.GetY() + x;
  }
}
class B
{
  int y = 1;
  int GetY()
  {
    return y;
  }
}
```
We schrijven tests voor A en B met mocks. Om het simpel te houden testen we als volgt:
1) We verifiëren dat B.GetY() gelijk is aan 1;
2) We verifiëren dat A.calculateXY() gelijk is aan 2;
Nu introduceren we een fout in de code:
```csharp
class A
{
  static int x = 1;
  static int calculateXY(B b)
  {
    return b.GetY() + x;
  }
}
class B
{
  static int y = 1;
  int GetY()
  {
    return y+1;
  }
}
```
Door het gebruik van mocks, faalt test 1 en slaagt test 2. Zo weten we dus meteen dat de fout in B.GetY zit.
####  Mocks gebruiken
##### Een Mock aanmaken
Basis syntax:
```csharp
var mock = new Mock<T>();
```
Let op: dit werkt alleen als de class T een constructor zonder parameters bevat. Indien T niet over zo'n constructor beschikt, moet je de parameters van de reguliere constructor van T specificeren:
```csharp
var mock = new Mock<T>(param1, param2);
```
##### Het gedrag van een mock bepalen
Terugkijkend op het voorbeeld van zojuist, kunnen we class B als volgt mocken, en de uitkomst van GetY bepalen:
```csharp
var mock = new Mock<B>();
mock.Setup(b => b.GetY()).Returns(1);
```
##### Een Mock gebruiken in je test
We kunnen met onze mocked B een test voor A.CalculateXY schrijven. We geven de mock mee als parameter. Let op: geef niet de mock zelf, maar mock.Object mee als parameter, anders krijg je een type error!
```csharp
Assert.AreEqual(2, A.CalculateXY(mock.Object));
```
Voor meer info is hier een gedetailleerde wiki pagina met veel operation op mocks:
[Moq wiki](https://github.com/Moq/moq4/wiki/Quickstart)
