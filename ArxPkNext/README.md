# ArxPkNext

Arxivar server plugin that handles form requests linked to a Laravel interface to handle form ingest and presentation to final users.

## Flow

The plugin creates a new server biding required host and port. The external processes should have the following flow:

1. User fills in form attaching the passport photo
2. Laravel interface produces a PDF based on the input
3. Laravel interface sends the encoded PDF and passport photo to Arxivar, this plugin
4. The data received by this plugin creates or links new Address Book entries and creates a new document Profile
5. Laravel interface redirects the user to the Signing Platform
6. The Signing Platform requests Profile infos and contents from Laravel interface, which proxies the reuests to this plugin
7. The document is signed on the Signing Platform
8. The Signing Platform send the signed document to the Laravel interface, which sends the encode document along with the new has to this plugin
9. This plugin updates the Profile acknowledging the Laravel interface
10. Laravel interface redirects the user to the thank-you page, completing the flow.

## Installation

To properly install this plugin you'll have to follow these steps:

1. Configure the Arxivar Server with the Document Type that will be used by the plugin. The Document Type is required to have the following Custom Fields:
   | Label            | SDK Alias          | Length | Required | Disabled |
   | ---------------- | ------------------ | ------ | -------- | -------- |
   | Hash SHA256      | `SHA256_HASH`      | 64     | ✅ / Yes | 🔒 / Yes |
   | Numero Richiesta | `NUMERO_RICHIESTA` | 24     | ✅ / Yes | 🔒 / Yes |
2. Configure Before and After document states
3. Configure the Address Book
4. Place this plugin under `RepoService\Plugin\` in the Arxivar Server's root directory, creating the missing directories if needed.
5. Copy or create a `ArxPkNext.dll.config` file and fill in the proper configuration.
6. Add a firewall rule for the listen port (default: 4321/TCP) to allow the new webserver to be reached from outside networks besides localhost.

Restarting the WCF Service should load the plugin and be ready to use it.
