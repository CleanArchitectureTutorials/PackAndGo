# Switch Environment Variable On and Off

set -a
source ./.env
set +a

```bash
export USE_REAL_DATABASE=true
export ENABLE_MANUAL_TEST_PAUSE=true
```

```bash
unset USE_REAL_DATABASE
unset ENABLE_MANUAL_TEST_PAUSE
```

