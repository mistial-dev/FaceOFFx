# Security Policy

## Supported Versions

We release patches for security vulnerabilities. Which versions are eligible for receiving such patches depends on the CVSS v3.0 Rating:

| Version | Supported          |
|---------|--------------------|
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take the security of FaceOFFx seriously. If you have discovered a security vulnerability in our project, we appreciate your help in disclosing it to us in a responsible manner.

### Reporting Process

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them via email to:

- <security@mistial.dev>

You should receive a response within 48 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

Please include the following information in your report:

- Type of issue (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
- Full paths of source file(s) related to the manifestation of the issue
- The location of the affected source code (tag/branch/commit or direct URL)
- Any special configuration required to reproduce the issue
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if possible)
- Impact of the issue, including how an attacker might exploit it

### What to Expect

When you report a vulnerability, you can expect:

1. **Acknowledgment**: We'll acknowledge receipt of your vulnerability report within 48 hours
2. **Assessment**: We'll assess the vulnerability and determine its severity
3. **Fix Development**: We'll develop a fix for confirmed vulnerabilities
4. **Coordination**: We'll coordinate with you regarding public disclosure timing
5. **Credit**: We'll credit you for the discovery if you wish (unless you prefer to remain anonymous)

### Disclosure Policy

We follow a coordinated disclosure policy:

- We will work with you to understand and validate the issue
- We will prepare a fix and release it as soon as possible
- We will publicly disclose the vulnerability after the fix is available
- We aim to resolve critical issues within 30 days
- We will credit researchers who report valid security issues (with permission)

## Security Best Practices for Users

### General Usage

1. **Keep Updated**: Always use the latest version of FaceOFFx
2. **Secure Storage**: Store processed images securely
3. **Access Control**: Implement proper access controls for image processing
4. **Audit Logging**: Enable logging for compliance and security monitoring
5. **Input Validation**: Validate all image inputs before processing

### If Using for Government ID Processing

If you are using FaceOFFx to process government ID photos or PIV cards, you should:

- Ensure proper authorization before processing PIV images
- Follow FIPS 201-3 guidelines for handling biometric data
- Implement appropriate data retention policies
- Use encrypted storage for processed images
- Maintain audit trails for compliance

## Security Features

FaceOFFx includes several security features:

- **Input Validation**: Validates image formats and dimensions
- **Memory Safety**: Uses managed memory through .NET
- **No External Network Calls**: All processing is done locally
- **Dependency Security**: Regular updates of dependencies
- **SBOM Generation**: Software Bill of Materials for supply chain transparency

## Known Security Considerations

### ONNX Model Security

- ONNX models are embedded as resources and cannot be tampered with at runtime
- Models are loaded from embedded resources only, not from external files
- Model inference runs in a sandboxed environment

### Image Processing

- All image processing is performed locally with validated inputs
- Input images are validated before processing
- Memory usage is bounded during processing

## Additional Resources

- [NIST PIV Standards](https://csrc.nist.gov/projects/piv/piv-standards-and-supporting-documentation)
- [OWASP Security Guidelines](https://owasp.org/www-project-application-security-verification-standard/)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)

## Contact

For any security-related questions that don't require immediate attention, you can also:

- Open a [GitHub Discussion](https://github.com/mistial-dev/FaceOFFx/discussions) with the "security" tag
- Check our [Contributing Guidelines](docs/CONTRIBUTING.md) for general questions

Thank you for helping keep FaceOFFx and its users safe!
