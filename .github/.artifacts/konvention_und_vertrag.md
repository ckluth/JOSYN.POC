# Konvention und Vertrag
### Eine ungleiche Ehe in der Softwareentwicklung

---

Es gibt zwei grundlegend verschiedene Arten, wie wir als Entwickler Erwartungen ausdrücken: als **Konvention** und als **Vertrag**. Der Unterschied klingt akademisch, ist aber einer der folgenreichsten Unterschiede im täglichen Handwerk.

---

## Was ist eine Konvention?

Eine Konvention ist eine stillschweigende Übereinkunft. Sie existiert im Kopf von Menschen, nicht im Compiler. Sie lebt im Onboarding-Gespräch, im Wiki-Eintrag, den niemand liest, und im Code-Review-Kommentar, der zur Floskel geworden ist. Konventionen brauchen *Eingeweihte* — Menschen, die sie kennen, verstehen und einhalten wollen.

Das ist kein Vorwurf. Konventionen sind unvermeidlich. Die Benennung von Methoden, Commit-Messages, Paketstrukturen, die Entscheidung, wann man einen Service schneidet und wann nicht — das alles ist Konvention. Und gute Konventionen sind nicht nichts. Sie tragen echte Weisheit, sind das Ergebnis von schmerzhafter Erfahrung und kollektiver Intelligenz.

Aber sie sind fragil. Ihr größtes Risiko ist nicht, dass sie gebrochen werden — es ist, dass sie **vergessen** werden. Wissen stirbt mit dem Team.

---

## Was ist ein Vertrag?

Ein Vertrag ist explizit, maschinenprüfbar und unabhängig von menschlicher Disziplin. Ein Interface in TypeScript, ein JSON Schema, ein OpenAPI-Spec, ein Typ in Haskell — das sind Verträge. Sie sagen nicht *„bitte mach das so"*, sie sagen *„wenn du das nicht so machst, kompiliert es nicht"* — oder: *„wenn du das nicht so machst, fliegt die Validierung zur Laufzeit auf"*.

Ein Interface ist deshalb kein Konvention-Ersatz, sondern ein fundamentaler **Kategoriewechsel**: von menschlicher Disziplin zu maschineller Überprüfung. Der Antagonismus zwischen beiden ist kein Zufall, er ist gewollt.

---

## Die Grauzone: wo Konvention gut und wo sie gefährlich ist

Nehmen wir ein konkretes Spektrum:

Eine **magic number, verteilt über hundert Dateien** — das ist schlechte Konvention. Es ist implizites Wissen (`42` bedeutet hier den Timeout-Wert in Sekunden), aber die Information ist nicht am Ort des Wissens. Wer die Konvention nicht kennt, versteht den Code nicht. Schlechter Geruch, zu Recht.

Dieselbe magic number als **benannte Konstante** `DEFAULT_TIMEOUT_SECONDS = 42` ist immer noch Konvention — aber eine lokalisierte, artikulierte. Der Name trägt die Semantik. Kein Smell mehr.

Eine **optionale Annotation** über einer Klasse — `@Deprecated`, `@Transactional`, `@JsonIgnore` — ist ebenfalls Konvention, aber eine, die ein Framework oder eine Runtime interpretiert. Sie bleibt menschlich-lesbar, gewinnt aber durch die Toolchain-Unterstützung Vertragscharakter. Kein Smell.

Und dann: **Code selbst**. Jede Zeile Code ist eigentlich Konvention — ein Requirement, das in Syntax gegossen wurde, von einem fehlbaren Menschen eingehalten werden soll. Es gibt keinen Compiler, der prüft, ob eine Businessregel korrekt implementiert ist. Der Code kompiliert — und ist trotzdem falsch. Das ist die tiefste und unbequemste Wahrheit:

> **Korrektheit ist immer Konvention.**

---

## Wo Konventionen legitim bleiben

Nicht alles lässt sich in Verträge gießen — und das wäre auch falsch. Architekturentscheidungen, Teamkultur, Coding-Style jenseits automatischer Formatter, die Entscheidung über Schnittmuster von Services — das bleibt Konvention, weil es menschliche Weisheit erfordert, die kein Typsystem ausdrücken kann.

*Convention over Configuration* — Rails, Spring Boot, viele moderne Frameworks — ist sogar ein explizites Designprinzip, das Produktivität gegen Explizitheit tauscht. Das ist keine Naivität, das ist ein bewusster Trade-off. Die Konvention kauft Geschwindigkeit. Der Preis ist: man muss in der Community bleiben, die die Konvention kennt.

---

## Der eigentliche Antagonismus

Der echte Gegner der Konvention ist nicht das Interface — es ist das **Vergessen**. Schlechte Konventionen sind nicht schlecht, weil sie Konventionen sind, sondern weil sie zu weit von der Information entfernt sind, die sie tragen, und weil sie zu stark vom Gedächtnis eines einzelnen abhängen.

Die handwerkliche Antwort ist: Was maschinenprüfbar gemacht werden kann, soll es werden. Was es nicht kann, braucht Nähe — zur Implementierung, zur Dokumentation, zur Disziplin.

Interfaces, Typen, Schemata, Contract Tests — das sind keine Feinde der Konvention. Sie sind ihre **Erlösung**. Der Schritt von der Konvention zum Vertrag ist immer ein Schritt weg von menschlicher Fehlerbarkeit hin zu struktureller Garantie.

Und das ist, kurz gesagt, der größte Teil dessen, was wir „gute Softwareentwicklung" nennen.

---

*Stammtisch-Notizen · Mai 2026*
